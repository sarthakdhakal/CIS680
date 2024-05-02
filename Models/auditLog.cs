using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class AuditLog
{
    public int EntryId { get; set; }

    public DateTime EntryTime { get; set; }

    public string EntrySource { get; set; } = null!;

    public string EntryStarid { get; set; } = null!;

    public string EntryText { get; set; } = null!;

    public string? ContentBefore { get; set; }

    public string? ContentAfter { get; set; }
}
