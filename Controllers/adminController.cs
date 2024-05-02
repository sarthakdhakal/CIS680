using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.Context;
using NewDotnet.DataLayer;
using NewDotnet.Models;
using Newtonsoft.Json;
using System.Security.Principal;


namespace NewDotnet.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class adminController : ControllerBase
    {
       
        private readonly Database db;
        public readonly OODBModelContext _context;
        private readonly OODBModel m;
        public adminController(OODBModelContext context)
        {

            _context = context;
            m = new OODBModel(_context);
            db = new Database(_context);
        }

        [HttpGet]
        [Route("getAnalytics/{int:id}&{int:playlistId}")]
        private object getAnalytics(int daysPast, int playlistId)
        {
           
            DateTime earliestTime = new DateTime(1970, 1, 1);
            if (daysPast > 0)
            {
                var ts = TimeSpan.FromDays(daysPast);
                earliestTime = DateTime.Now.Subtract(ts);
            }

            // 03-20-2018 - New method using Entity to get analytics view
            var analyticsQuery = _context.Assignments.Where(x => x.AssignedPlaylist == playlistId && x.EndTime > earliestTime).OrderByDescending(x => x.EndTime ?? DateTime.MaxValue);

            // Genericizing: We're only returning a baseline set of results. It will be up to the site code to return the specifics for each site and add them
            // to the frontend.

            // 3-25-19 - Exclude admins from the query
            var resultSet = analyticsQuery.Where(x => !m.IsAdmin(x.StarId)).Select(x => new {
                userId = x.StarId.Trim(),
                name = x.Account.LastName.Trim() + ", " + x.Account.FirstName.Trim(),
                x.EndTime,
                quizAttempts = x.Account.AccountQuizzes.Count
            }).ToList();

            return resultSet;

        }

#if DEBUG

        [HttpGet]
        [Route("exception")]
        public IActionResult ThrowException()
        {
            throw new InvalidOperationException("Throwing an exception as requested.", new InvalidOperationException("The exception has an inner exception."));
        }
#endif

        [HttpPost]
        
        [Route("assignUsers")]
        public IActionResult AssignUsers([FromBody] string[] userIds)
        {
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal((System.Security.Claims.ClaimsPrincipal)User);
          
            try
            {
                var results = db.AssignUsers(userIds, tc.Playlist, false);
                m.LogAuditEvent("admin/assign", tc.StarId, $"successfully assigned {userIds.Length} users to playlist {tc.Playlist}.", JsonConvert.SerializeObject(userIds), null, true);
                return Ok( new { error = 0, data = results });
            }
            catch (Exception ex)
            {
                m.LogAuditEvent("admin/assign", tc.StarId, $"exception {ex.GetType().Name} occurred: {ex.Message}", true);
                return BadRequest( new { error = 500, message = $"{ex.GetType().Name}: {ex.Message}" });
            }
        }
    }
}