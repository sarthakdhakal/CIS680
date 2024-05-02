using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class DjangoSession
{
    public string SessionKey { get; set; } = null!;

    public string SessionData { get; set; } = null!;

    public DateTimeOffset ExpireDate { get; set; }
}
