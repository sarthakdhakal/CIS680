using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.DataLayer;
using NewDotnet.Models;
using Newtonsoft.Json;
using System.Net;

namespace NewDotnet.Controllers
{
    public partial class QuizController
    {

        [HttpPost]
       
        [Route("updateQuiz/{id:int}")]
        public IActionResult UpdateQuiz(int id, [FromBody] QuestionUpdate[] newQuiz)
        {

            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            var m = new OODBModel(_context);

            // is this a valid section
            var sectionCheck = from x in _context.Sections where x.SectionId == id select x;
            if (!sectionCheck.Any())
            {
                return NotFound( new
                {
                    error = 404,
                    message =
                    $"No such section {id}."
                });
            }

            // delete all questions for this section
            var allQuestionsForSection = from x in _context.Questions where x.SectionId == id select x;
            /*
            if (allQuestionsForSection.Count() < 1)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new { error = 400, message = String.Format("No quiz currently exists for section {0}.", id) });
            }
            */

            string beforeContent = JsonConvert.SerializeObject(allQuestionsForSection.ToArray());
            _context.Questions.RemoveRange(allQuestionsForSection);

            int initialQuizId = id * 100;
            // some sanity fixes - make sure all questions belong to the correct ID
            var newQuizData = (from x in newQuiz
                               select new Question()
                               {
                                   QuestionId = initialQuizId++,
                                   SectionId = id,
                                   QuestionAnswers = String.Join("|", x.QuestionAnswers),
                                   QuestionText = x.QuestionText
                               }).ToArray();

            _context.Questions.AddRange(newQuizData);

            // KLUDGE: fix later when schema is updated.
            // update playlist to ensure quiz is present
            Database db = new Database(_context);
            db.EnsureQuizIsInPlaylist(tc.Playlist, id);

            m.LogAuditEvent("quiz/edit", tc.StarId, "added or edited quiz " + id.ToString(), beforeContent, JsonConvert.SerializeObject(newQuiz), false);
            _context.SaveChanges();
            return Ok( new { error = 0 });

        }

        [HttpDelete]
        
        [Route("deleteQuiz/{id:int}")]
        public IActionResult DeleteQuiz(int id)
        {

            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            var m = new OODBModel(_context);

            // is this a valid section
            var sectionCheck = from x in _context.Sections where x.SectionId == id select x;
            if (!sectionCheck.Any())
            {
                return NotFound( new
                {
                    error = 404,
                    message =
                    $"No such section {id}."
                });
            }   

            // delete all questions for this section
            var allQuestionsForSection = from x in _context.Questions where x.SectionId == id select x;
            if (!allQuestionsForSection.Any())
            {
                return NotFound( new
                {
                    error = 400,
                    message =
                    $"No quiz currently exists for section {id}."
                });
            }

            m.LogAuditEvent("quiz/delete", tc.StarId, "deleted quiz for section " + id, JsonConvert.SerializeObject(allQuestionsForSection.ToArray()), null, false);
            _context.Questions.RemoveRange(allQuestionsForSection);

            Database db = new Database(_context);
            db.EnsureQuizIsNotInPlaylist(tc.Playlist, id);

            _context.SaveChanges();
            return Ok( new { error = 0 });
        }
    }
}
