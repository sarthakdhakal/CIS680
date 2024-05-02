using NewDotnet.Models;

namespace NewDotnet.Authentication
{
    public interface IOrientationAuthProvider
    {
        bool Authenticate(string username, string password, out AuthenticatedUserInformation info);
    }

    public class AuthenticatedUserInformation
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string UserId { get; set; }
    }
}
