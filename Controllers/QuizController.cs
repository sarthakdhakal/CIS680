using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.Context;
using NewDotnet.Models;
using System.Linq;
using System.Net;


namespace NewDotnet.Controllers
{
    [ApiController]
    [Route("api/quiz")]
    public partial class QuizController : ControllerBase
    {
        // Provides access to the data model
        private readonly OODBModelContext _context;

        public QuizController(OODBModelContext context)
        {
            // initialize connection to data model
            _context = context;
        ;
        }
        private static int GetCorrectAnswer(int seed, Question question)
        {

            long longSeed = (seed + question.QuestionId) % ((long)int.MaxValue + 1);

            // Create a new random number generator based on the seed plus the question ID.
            Random r = new Random((int)longSeed);

            // Create a list of ints containing sequential numbers starting from 0.
            List<int> newOrder = Enumerable.Range(0, question.QuestionAnswers.Split('|').Length).ToList();

            // Randomize the list of ints based on the random seed.
            int[] result = (from i in newOrder orderby r.Next() select i).ToArray();

            // Find where answer '0' is.
            int answer = Array.IndexOf(result, 0);

            // This should never happen! (We based the new array on a generated array that should have contained a 0.)
            if (answer == -1)
                throw new ApplicationException("An inconsistency was detected. This error is FATAL.");

            // The index of the "0" in the array is the index of the correct answer.
            return answer;
        }

        private static string GetRandomizedQuestionAnswers(int seed, Question q)
        {
            // do not randomize if seed is 0 (e.g. for admins)
            if (seed == 0) return q.QuestionAnswers;

            long longSeed = (seed + q.QuestionId) % ((long)int.MaxValue + 1);

            string[] allQuestionAnswers = q.QuestionAnswers.Split('|');
            Random r = new Random((int)longSeed);
            string[] result = (from i in allQuestionAnswers orderby r.Next() select i).ToArray();
            return string.Join("|", result);
        }

        [HttpGet]

        [Route("getQuizStatues")]
        public IActionResult GetQuizStatuses()
        {
            // Check the user ID
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            var m = new OODBModel(_context);

            var ep = m.GetUserSpecificPlaylist(tc.StarId, tc.Playlist).ToList();

            // get all sections for all quizzes this user should see
            var localSectionCopy = (from x in  _context.Sections select x).ToList();

            // get all section IDs which contain a quiz
            var quizIds = _context.Questions.Select(x => x.SectionId).Distinct().ToList();

            var allSections = ep.Where(x => quizIds.Contains(x.SectionId.GetValueOrDefault(-1))).Select(x => new { id = x.SectionId, title = x.SectionTitle }).Distinct();

            // Is this user view only?
            if (m.IsComplete(tc))
            {
                var completedSections = allSections.Select(x => new { sectionId = x.id, sectionTitle = x.title, hasPassed = "yes" }).ToList();

                return Ok( new
                {
                    error = 0,
                    data = new
                    {
                        totalQuizzes = completedSections.Count,
                        totalPasses = completedSections.Count,
                        details = completedSections
                    }
                });
            }

            // now get all data on this user's successful quiz attempts
            var allSuccess = _context.AccountQuizzes.Where(x => x.StarId == tc.StarId && x.AccountPassed).Select(x => x.Section).ToList();

            // now generate the return data
            var returnDataSections = from x in allSections
                                     select new
                                     {
                                         sectionId = x.id,
                                         sectionTitle = x.title,
                                         hasPassed = "yes",
                                     };

            // how many quizzes?
            var dataSections = returnDataSections.ToList();
            int howManyQuizzes = dataSections.Count;

            // how many passes?
            int howManyPasses = dataSections.Where(x => x.hasPassed == "yes").Select(x => 0).Count();

            return Ok( new { error = 0, data = new { totalQuizzes = howManyQuizzes, totalPasses = howManyPasses, details = dataSections } });
        }
    }

}