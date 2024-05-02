using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.Context;
using NewDotnet.Models;



namespace NewDotnet.Controllers
{
    [ApiController]
    [Route("api/cohort")]
    public partial class CohortController : ControllerBase
    {
        private readonly OODBModelContext _context;

        public CohortController(OODBModelContext context)
        {
           _context = context;
        }

        [HttpGet("getselectionscreen/{id:int}")]
        public IActionResult GetSelectionScreen(int id)
        {
           
            var btc = Services.GetTokenDataFromUserPrincipal(User);

            var cohortQuestions = _context.CohortPrompts.Where(x => x.PlaylistId == id).OrderBy(x => x.PromptOrder).ToList();
            if (!cohortQuestions.Any())
            {
                return NotFound(new { error = 404, message = $"No student status selection screen available for playlist {id}." });
            }

            var finalQuiz = cohortQuestions.Select(x => new { question_id = x.PromptOrder, question_text = x.PromptText, quiz_id = id, question_answers = x.PromptOptions.Split('|') });

            return Ok(new { error = 0, data = new { seed = 0, questions = finalQuiz } });
        }

        [HttpPost("enrollCohorts/{id:int}")]
        public IActionResult EnrollCohorts(int id, [FromBody] QuizResponse qResponse)
        {
            var m = new OODBModel(_context);
            var btc = Services.GetTokenDataFromUserPrincipal(User);

            var cohortQuestions = _context.CohortPrompts.Where(x => x.PlaylistId == id).OrderBy(x => x.PromptOrder).ToList();
            if (!cohortQuestions.Any())
            {
                m.LogAuditEvent("cohort/enroll", btc.StarId, $"request for nonexistent status selection list for playlist {id}.", true);
                return NotFound(new { error = 404, message = $"No student status selection screen available for playlist {id}." });
            }

            if (cohortQuestions.Count != qResponse.Answers.Length)
            {
                m.LogAuditEvent("cohort/enroll", btc.StarId, $"Invalid submission for cohort enrollment for playlist {id} (expected {cohortQuestions.Count} responses, got {qResponse.Answers.Length}).", true);
                return BadRequest(new { error = 400, message = "Invalid submission." });
            }

            var account = _context.Accounts.FirstOrDefault(x => x.StarId == btc.StarId);
            if (account == null)
            {
                return NotFound(new { error = 404, message = "Account not found." });
            }

            // Clear all cohort flags from the user if needed
            // ...

            // Enroll the user in the selected cohorts
            // ...

            // For now, remove all cohort flags from the user.
            foreach (Flag f in account.Flags.ToArray())
            {
                if (f.FlagType == "cohort")
                {
                    account.Flags.Remove(f);
                }
            }

            // Iterate through the responses and enroll the user in the selected cohorts.
            for (int i = 0; i < qResponse.Answers.Length; i++)
            {
                // Determine the actual flag ID of this response
                // Get flag IDs for this question
                int[] flagsByIndex = cohortQuestions.ToArray()[i].CohortMap.Split('|').Select(x => Services.TryParseWithDefault(x, -1)).ToArray();
                int thisCohortFlag = flagsByIndex[qResponse.Answers[i]];

                // If the cohort flag returned is -1, then we will ignore this cohort.
                if (thisCohortFlag == -1) continue;
                // Otherwise...
                // Now, assign this user to the cohort.
                var thisFlag = _context.Flags.First(x => x.FlagId == thisCohortFlag);
                if (!account.Flags.Contains(thisFlag))
                    account.Flags.Add(thisFlag);
            }
            m.LogAuditEvent("cohort/enroll", btc.StarId, $"enrolled or updated {qResponse.Answers.Length} cohorts.");

            _context.SaveChanges();

            return Ok(new { error = 0 });
        }
    }
}
