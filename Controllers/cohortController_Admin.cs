using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NewDotnet.Code;
using NewDotnet.Models;


namespace NewDotnet.Controllers
{
  
        public class NewCohort
        {
            public string name { get; set; }
            public string description { get; set; }
        }
        public class UserUpdate
        {
            public string userId { get; set; }
            public int[] cohorts { get; set; }
        }
    public partial class CohortController
    {


        [HttpPost]
        
        [Route("updateuserCohorts")]
        public IActionResult UpdateUserCohorts([FromBody] UserUpdate u)
        {
            // Look for user
            var thisUser = _context.Accounts.Where(x => x.StarId == u.userId).FirstOrDefault();
            if (thisUser == null) return NotFound(new { error = 404, message = "User not found." });

            thisUser.Flags.Clear();

            // Iterate cohorts and add to user
            foreach (int cId in u.cohorts)
            {
                var thisCohort = _context.Flags.Where(x => x.FlagType == "cohort" && x.FlagId == cId).FirstOrDefault();
                if (thisCohort == null)
                {
                    return NotFound(new { error = 404, message = $"No such cohort with id {cId}." });
                }
                thisUser.Flags.Add(thisCohort);
            }

            _context.SaveChanges();
            return Ok(); 
        }

        [HttpDelete]
       
        [Route("delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            try{
            // ASSUMING THAT FRONTEND HAS CONFIRMED COHORT REMOVAL - THIS METHOD DOES NOT CHECK!
            var m = new OODBModel(_context);
            var thisFlag = _context.Flags.Where(x => x.FlagId == id).FirstOrDefault();
            if (thisFlag == null) return NotFound(new { error = 404, message = "Cohort not found." });
            // Remove from all slides
            foreach (var c in thisFlag.Contents)
            {
                c.Flags.Remove(thisFlag);
            }

            // Remove from all users
            foreach (var a in thisFlag.Accounts)
            {
                a.Flags.Remove(thisFlag);
            }

            // Remove from all sections
            foreach (var s in thisFlag.Sections)
            {
                s.Flags.Remove(thisFlag);
            }

            // Remove the flag
            _context.Flags.Remove(thisFlag);

            m.LogAuditEvent("cohort/delete", Services.GetTokenDataFromUserPrincipal((System.Security.Claims.ClaimsPrincipal)User).StarId, $"deleted cohort {id} ({thisFlag.FlagName})", true);

            _context.SaveChanges();
            }
            catch(Exception ex){
                return BadRequest(new{error = ex.GetBaseException().Message});
            }
            return Ok();    
        }



        [HttpPut]

        [Route("newCohort")]
        public IActionResult Create([FromBody] NewCohort newCohort)
        {
            // See if cohort with same name already exists
            var testForCohort = _context.Flags.Where(x => x.FlagType == "cohort" && x.FlagName.ToLower() == newCohort.name.ToLower()).FirstOrDefault();
            if (testForCohort != null)
            {
                return BadRequest (new { error = 400, message = "There is already a cohort with the name '" + newCohort + "'." });
            }
            // Build a new flag
            Flag f = new Flag
            {
                FlagName = newCohort.name,
                FlagDescription = newCohort.description,
                FlagType = "cohort",
                FlagHelpText = "Cohort added by " + Services.GetTokenDataFromUserPrincipal((System.Security.Claims.ClaimsPrincipal)User).StarId
            };
            _context.Flags.Add(f);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
    
        [Route("addCohortstocontent/{id:int}")]
        public IActionResult addCohortsToContent(int id, [FromBody] int[] ids)
        {
            var m = new OODBModel(_context);

            // Try to get content
            var thisContent = _context.Contents.Where(x => x.ContentId == id).FirstOrDefault();
            if (thisContent == null)
            {
                return NotFound( new { error = 404, message = "No such content." });
            }

            foreach (var existingCohort in thisContent.Flags.Where(x => x.FlagType == "cohort").ToList())
            {
                thisContent.Flags.Remove(existingCohort);
            }

            // Try to iterate through each flag
            foreach (int f in ids)
            {
                var thisFlag = _context.Flags.Where(x => x.FlagId == f && x.FlagType == "cohort").FirstOrDefault();
                if (thisFlag == null)
                {
                    return NotFound( new { error = 404, message = $"No such flag {f} or flag {f} is not a cohort flag." });
                }
                thisContent.Flags.Add(thisFlag);
            }

            // All worked.
            _context.SaveChanges();

            string cohortIdString = string.Join(", ", ids.Select(x => x.ToString()).ToArray());

            m.LogAuditEvent("cohort/content", Services.GetTokenDataFromUserPrincipal((System.Security.Claims.ClaimsPrincipal)User).StarId, $"added these cohorts to content {id}: {cohortIdString}");

            return Ok();

        }
        [HttpGet]
        
        [Route("getAvailableFlags")]
        public IActionResult getAvailableFlags()
        {
            var m = new OODBModel(_context);

            // test code for cohorts
            var allFlags = _context.Flags.Where(x => x.FlagType == "cohort").Select(x => new { x.FlagId, flagName = x.FlagDescription });

            m.LogAuditEvent("cohort/list", Services.GetTokenDataFromUserPrincipal((System.Security.Claims.ClaimsPrincipal)User).StarId, "retrieved list of all cohorts.");

            return Ok( new { error = 0, data = allFlags });
        }
    }
}
