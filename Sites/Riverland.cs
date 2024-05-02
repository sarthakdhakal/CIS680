using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewDotnet.Code;
using NewDotnet.Context;
using NewDotnet.Models;
using System.Linq;

namespace NewDotnet.Sites
{
    [ApiController]
    [Route("api/site/riverland")]
    public class RiverlandSiteController : ControllerBase
    {
        private readonly OODBModelContext _context;

        public RiverlandSiteController(OODBModelContext context)
        {
            _context = context;
        }

        [HttpGet("analytics")]
        public ActionResult SiteAnalytics()
        {
            // Use User.Claims to access user principal data if using authentication middleware
            var btc = Services.GetTokenDataFromUserPrincipal(User);

            // EF Core supports lazy loading, ensure it is configured if needed or use Include to load related data
            var baseQuery = _context.Assignments
                                    .Where(x => x.AssignedPlaylist == 0)
                                    .Include(x => x.Account)
                                    .ToList();

            Dictionary<string, string> advisors = new Dictionary<string, string>();
            foreach (var asgn in baseQuery)
            {
                var advisorCohort = asgn.Account.Flags.FirstOrDefault(x => x.FlagName.StartsWith("adv_"));
                string advName = advisorCohort != null ? advisorCohort.FlagDescription : "(unknown)";
                advisors.Add(asgn.StarId, advName);
            }

            string starIds = string.Join(", ", baseQuery.Select(x => $"'{x.StarId}'"));
            
            var techIds =  _context.Accounts
                .Where(a => starIds.Contains(a.StarId))
                .ToDictionary(a => a.StarId, a => a.StarId);
            // Execute raw SQL queries carefully to avoid SQL injection
          
            var entireSeries = baseQuery.Select(x => new
            {
                userId = x.StarId,
               
            }).ToList();

            var headers = new List<string[]>
            {
             
            };

            return Ok(new
            {
                error = 0,
                data = new
                {
                    headers,
                    rows = entireSeries
                }
            });
        }

        public class StarIdTechIdMapping
        {
            public string StarId { get; set; }
            public string TechId { get; set; }
        }
    }
}
