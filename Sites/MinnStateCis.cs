
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewDotnet.Code;
using NewDotnet.Context;
using NewDotnet.Models;

namespace NewDotnet.Sites
{
    [ApiController]
    [Route("api/site/minnstate-cis")]
    public class MnsuCISSiteController : ControllerBase
    {
        private readonly OODBModelContext _context;
        public MnsuCISSiteController(OODBModelContext context)
        {
            _context = context;
        }




        [HttpGet("analytics")]

        public async Task<ActionResult<object>> SiteAnalytics()
        {
            OODBModel m = new OODBModel(_context);
            // Assuming getTokenDataFromUserPrincipal is part of Services and correctly handles user principal extraction
            var userTokenData = Services.GetTokenDataFromUserPrincipal(User);

            var flagIdForMilitary = await _context.Flags
                .Where(f => f.FlagName == "oe_veteran")
                .Select(f => f.FlagId)
                .FirstOrDefaultAsync();

            var assignments = await _context.Assignments
                .Where(a => a.AssignedPlaylist == 0)
                .ToListAsync();

            var starIds = assignments.Select(a => a.StarId).Distinct().ToList();

            // Perform an explicit join between Accounts and the StarIds.
            var accounts = await _context.Accounts
                .Join(starIds,
                      account => account.StarId,  // Key from Accounts
                      starId => starId  ,           // Key from starIds list
                      (account, starId) => account) // Result selector
                .DistinctBy(account => account.StarId) // Ensure each StarId is unique to avoid duplicate key exception
                .ToDictionaryAsync(account => account.StarId, account => account.StarId);


            var result = assignments.Select(a => new
            {
                userId = a.StarId,
                techId = accounts.ContainsKey(a.StarId) ? accounts[a.StarId] : "N/A"
            }).ToList();

            return Ok(new
            {
                error = 0,
                data = new
                {
                    headers = new List<string[]> { new string[] { "techId", "Tech ID" } },
                    rows = result
                }
            });
        }

        /* [HttpGet("getAccessCodeForStarId")]
         [AdminMethod]
         public  string GetAccessCodeForStarId(string starId)
         {
             OODBModel m = new OODBModel(_context);
             var academicTermId =  _context.Sections
                 .Where(s => s.B < DateTime.Now)
                 .OrderByDescending(s => s.BeginOfSemesterDate)
                 .Select(s => s.AcademicTerm)
                 .FirstOrDefault();

             if (string.IsNullOrEmpty(academicTermId))
                 return null;

             int academicTermIdInt = int.Parse(academicTermId);

             var techId =  GetTechIdForStarId(starId);
             if (techId == null) return null;

             var accessCode =  _context.StudentAccessCode
                 .Where(s => s.StudentId == techId && int.Parse(s.AcademicTerm) > academicTermIdInt)
                 .OrderBy(s => s.AcademicTerm)
                 .Select(s => s.AccessCode)
                 .FirstOrDefault();

             return accessCode;
         }*/

        /*  [HttpGet("getTechIdForStarId")]
          [AdminMethod]
          public  string GetTechIdForStarId(string starId)
          {
              OODBModel m = new OODBModel(_context);
              return  _context.Accounts
                  .Where(a => a.starId == starId)
                  .Select(a => a.techId)
                  .FirstOrDefault();
          }*/
        /*[HttpGet("getAcademicTermName")]
        [AdminMethod]
        public  string GetAcademicTermName(string academicTermId)
        {
            OODBModel m = new OODBModel(_context);
            return  _context.Semesters
                .Where(s => s.AcademicTerm == academicTermId)
                .Select(s => s.SemesterName + " " + s.Year)
                .FirstOrDefault();
        }*/


    }
    public class StarIdTechIdMapping
    {
        public string starId { get; set; }
        public string techId { get; set; }
    }
}


