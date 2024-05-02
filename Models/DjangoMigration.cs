using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class DjangoMigration
{
    public long Id { get; set; }

    public string App { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTimeOffset Applied { get; set; }
}
