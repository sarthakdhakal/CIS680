using NewDotnet.Context;
using NewDotnet.Models;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

namespace NewDotnet.Authentication
{
    public class MinnStateAuthProvider : IOrientationAuthProvider
    {
        public bool DeveloperMode { get; set; }
     
        public string GuestPassword { get; set; }
        private readonly OODBModelContext _context;


        public MinnStateAuthProvider(bool developerMode, string guestPassword, OODBModelContext context)
        {
            DeveloperMode = developerMode;
            GuestPassword = guestPassword;
            _context = context;
            
        }
    


        public bool Authenticate(string username, string password, out AuthenticatedUserInformation info)
        {
            info = null;
            string authenticationEndpoint = "https://b12dapi.campus.mnsu.edu/soidentity/api/account/token";
            // should we skip authentication because the username is clearly not a valid StarID?
            Match testCheck = Regex.Match(username, "^[a-z]{2}[0-9]{4}[a-z]{2}$|^guest$");
            if (testCheck.Success == false)
            {
                LogEvent(username, "login failed: username is not a valid StarID");
                //m.logAuditingEvent("login", username, "invalid StarID.", true);
                //context.SetError("invalid_grant", "The StarID or password is incorrect."); // show a generic error to the user, even though we know what happened...
                //return Task.FromResult<object>(null);
                return false;
            }

            if (DeveloperMode)
            {
                testCheck = Regex.Match(username, "^(te00[0-9]{2}st)|tu00[0-9]{2}to|ad000[0-9]mi|guest$");
                if (testCheck.Success)
                {
                    OODBModel m = new OODBModel(_context);
                    string[] acc = [username, "Developer"];
                    string email = username + "@orientation.dev";
                    info = new AuthenticatedUserInformation();
                    info.FirstName = acc[0];
                    info.LastName = acc[1];
                    info.EmailAddress = email;
                    LogEvent(username, "login succeeded: valid developer account.");
                    return true;
                }
            }
            if (username == "guest" && password == GuestPassword)
            {
                // check if guests may login
                //if (m.getConfigurationValue("allowGuestLogin") != "1")
                //{
                //    context.SetError("invalid_grant", "The StarID or password is incorrect."); // show a generic error to the user, even though we know what happened...
                //    m.logAuditingEvent("login", context.UserName, "guest login attempted but guest logins are disabled in configuration");
                //    return Task.FromResult<object>(null);
                //}
                return true;
            }
                HttpClient httpClient = new HttpClient();
                int statusCode;
                try
              {
                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
                });

                var task=  httpClient.PostAsJsonAsync(authenticationEndpoint, content);
                var response = task.Result;
                if (response.IsSuccessStatusCode)
                {
                    var tokenJson =  response.Content.ReadAsStringAsync();
                    var tokenData = JObject.Parse(tokenJson.Result);
                    var jwt = new JwtSecurityToken(tokenData["AccessToken"].ToString());

                    var realNameClaim = jwt.Claims.FirstOrDefault(x => x.Type == "http://www.minnstate.edu/claims/customclaims/displayname");
                    var emailClaim = jwt.Claims.FirstOrDefault(x => x.Type == "http://www.minnstate.edu/claims/customclaims/mnscuemailaddresslist");

                    info = new AuthenticatedUserInformation
                    {
                        EmailAddress = emailClaim?.Value,
                        FirstName = realNameClaim?.Value.Split(' ')[0],
                        LastName = realNameClaim?.Value.Substring(realNameClaim.Value.IndexOf(' ')).Trim()
                    };
                    LogEvent(username, "login succeeded.");
                    return true;
                }
                else
                {
                    LogEvent(username, "login failed: incorrect StarID or password.");
                    return false;
                }
            }
            catch (Exception e)
            {
                LogEvent(username, $"login failed: authenticating against StarID provider caused {e.GetType()}: {e.Message}");
                return false;
            }
        }

       
            private void LogEvent(string username, string eventText)
        {
            OODBModel m = new OODBModel(_context);
            m.LogAuditEvent("login", username, eventText, true);
            _context.SaveChanges();
        }

      
    }

}
