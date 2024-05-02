using Microsoft.EntityFrameworkCore;
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
    public class MnsuAuthProvider : IOrientationAuthProvider
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public bool DeveloperMode { get; set; }
     
        public string GuestPassword { get; set; }
        private readonly OODBModelContext _context;
       

        public MnsuAuthProvider(bool developerMode, string guestPassword, OODBModelContext context)
        {
            DeveloperMode = developerMode;
            GuestPassword = guestPassword;
            _context = context;

        }

        public bool Authenticate(string username, string password, out AuthenticatedUserInformation info)
        {
            info = null;
            OODBModel m = new OODBModel(_context);
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
                    
                    string[] acc = m.GetNames(username.ToLower());
                    string email = m.GetEmailAddress(username.ToLower());
                    info = new AuthenticatedUserInformation();
                    info.FirstName = acc[0];
                    info.LastName = acc[1];
                    info.EmailAddress = email;
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
            
            int statusCode;
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", HttpUtility.UrlEncode(password))
            });

                var data =  _httpClient.PostAsync("https://secure2.mnsu.edu/identity/oauth/token", content);
                var response = data.Result;
                if (response.IsSuccessStatusCode)
                {
                    // Example of how you might retrieve user information from your database
                    
                    string[] acc = m.GetNames(username.ToLower());
                    string email = m.GetEmailAddress(username.ToLower());

                    info = new AuthenticatedUserInformation
                    {
                        FirstName = acc[0],
                        LastName = acc[1],
                        EmailAddress = email
                    };
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
                LogEvent(username, "Error during authentication");
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
