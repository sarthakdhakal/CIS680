using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.Context;
using NewDotnet.DataLayer;
using NewDotnet.Models;
using System.Net;

namespace NewDotnet.Controllers
{
    [ApiController]
    [Route("api/user")]
    public partial class UserController : ControllerBase
    {
        // Provides access to the data model
        
        private readonly Database db;
        private readonly OODBModelContext _context; 
        // Constructor
        public UserController(OODBModelContext context)
        {
            // Make a connection to the database and store it in the class variable.
            
            _context = context;
            db = new Database(_context);
        }

        [HttpGet]
        [Route("getUserInfo")]
        public IActionResult getUserInfo()
        {
            var m = new OODBModel(_context);
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            if (tc.GuestId != "")
            {
                m.LogAuditEvent("user/get", (tc.GuestId == "" ? tc.StarId : "guest:" + tc.GuestId), "retrieved user information.", true);
                return Ok( new { error = 0, data = new { starId = "guest", name = "Guest", canSkip = true } });
            }
            var userData = _context.Accounts.Where(x => x.StarId == tc.StarId);
            if (!userData.Any())
            {
                return NotFound( new { error = 404, message = "No user data found." });
            }
            var userData1 = userData.First();
            string fullName = userData1.LastName + ", " + userData1.FirstName;
            m.LogAuditEvent("user/get", (tc.GuestId == "" ? tc.StarId : "guest:" + tc.GuestId), "retrieved user information.", true);
            return Ok( new { error = 0, data = new { userData1.StarId, name = fullName, canSkip = m.IsComplete(tc).ToString() } });
        }
    }
}


