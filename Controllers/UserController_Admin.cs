using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.DataLayer;
using NewDotnet.Models;
using Newtonsoft.Json;
using System.Net;

namespace NewDotnet.Controllers
{
    public partial class UserController
    {
        [HttpGet]
       
        [Route("getUserCohorts/{id}")]
        public IActionResult GetUserCohorts(string id)
        {
            var m = new OODBModel(_context);
            Account a = db.findAccount(id);
            if (a == null) return NotFound( new { error = 404, message = "No such user." });
            UserCohortResult ucr = new UserCohortResult
            {
                userId = a.StarId,
                techId = m.GetTechId(a.StarId),
                fullName = string.Join(", ", m.GetNames(a.StarId)),
                cohorts = a.Flags.Where(x => x.FlagType == "cohort").Select(x => x.FlagId).ToArray()
            };
            return Ok( new { error = 0, data = ucr });
        }

        [HttpPut]
      
        [Route("setPassword/{id:int}")]
        public IActionResult SetLocalUserPassword(int id, [FromBody] string newPassword)
        {
            var m = new OODBModel(_context);
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            db.SetUserPassword(id, newPassword);
            m.LogAuditEvent("user/password", tc.StarId, $"reset password for uid {id}", null, null, true);
            return Ok();
        }

        [HttpGet]
       
        [Route("getAllLocalUsers")]
        public IActionResult GetAllLocalUsers()
        {
            var m = new OODBModel(_context);
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            m.LogAuditEvent("user/list", tc.StarId, "list local users", null, null, true);

            var response = db.ListUsers();
            return Ok( new { error = 0, data = response });
        }

        [HttpPost]
     
        [Route("addLocalUser")]
        public IActionResult AddLocalUser([FromBody] LocalUserSubmission s)
        {
            var m = new OODBModel(_context);
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            m.LogAuditEvent("user/add", tc.StarId, "add local user " + s.UserId, JsonConvert.SerializeObject(s), null, true);

            db.AddUser(s.UserId, s.FirstName, s.LastName, s.PasswordInClearText, s.IsAdmin);
            db.AssignUsers(new string[] { s.UserId }, tc.Playlist, false); // assign the new user to the current playlist

            return Ok( new { error = 0 });
        }

        [HttpPost]
      
        [Route("local/enable")]
        public IActionResult SetLocalUserEnable([FromBody] bool enable)
        {
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            var m = new OODBModel(_context);
            if (enable)
            {
                m.LogAuditEvent("user/enable", tc.StarId, "enabling local users", null, null, true);
                db.SetConfigurationValue("allowLocalUsers", "1");
            }
            else
            {
                m.LogAuditEvent("user/disable", tc.StarId, "disabling local users", null, null, true);
                db.SetConfigurationValue("allowLocalUsers", "0");
            }
            return Ok();
        }

        [HttpGet]
   
        [Route("local/enable")]
        public IActionResult GetLocalUserEnable()
        {
            string result = db.GetConfigurationValue("allowLocalUsers");
            return Ok( new { error = 0, data = (result == "1") });
        }

        [HttpDelete]
        
        [Route("deleteLocalUser/{id}")]
        public IActionResult DeleteLocalUser(int id)
        {
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            var m = new OODBModel(_context);
            // does the user exist?
            var userAcct = _context.Users.FirstOrDefault(x => x.Id == id);

            if (userAcct == null)
            {
                return NotFound( new { error = 404, message = "No such user." });
            }

            m.LogAuditEvent("user/delete", tc.StarId, "delete local user user " + userAcct.UserId, JsonConvert.SerializeObject(userAcct), null, true);

            // Delete items
            db.DeleteUser(id);

            return  Ok (new { error = 0 });
        }

        [HttpDelete]
        
        [Route("restuser/{starId}")]
        public IActionResult ResetUser(string starId)
        {
            var m = new OODBModel(_context);

            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);

            if (starId.ToLower() == tc.StarId.ToLower())
            {
                return BadRequest( new { error = 400, message = "You cannot delete yourself!" });
            }

            // does the user exist?
            var userAcct = _context.Accounts.FirstOrDefault(x => x.StarId == starId);

            if (userAcct == null)
            {
                return NotFound( new { error = 404, message = "No such user." });
            }

            m.LogAuditEvent("user/reset", tc.StarId, "reset user " + userAcct.StarId, JsonConvert.SerializeObject(userAcct), null, true);

            db.ResetUser(userAcct.StarId, tc.Playlist);

            return Ok( new { error = 0 });
        }
    }

    public class LocalUserSubmission
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordInClearText { get; set; }
        public bool IsAdmin { get; set; }
    }

}
