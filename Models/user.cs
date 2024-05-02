using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class User
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public bool IsAdmin { get; set; }
}

public class AppSettings
{
    public string allowedCorsOrigins { get; set; }
    public string dbNamespace { get; set; }
    public string defaultCohort { get; set; }
    public string authenticationEndpoint { get; set; }
    public string guestPassword { get; set; }
    public string dummy { get; set; }

}