using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class Flag
{
    public int FlagId { get; set; }

    public string FlagType { get; set; } = null!;

    public string FlagName { get; set; } = null!;

    public string? FlagDescription { get; set; }

    public string? FlagHelpText { get; set; }

    public int? FlagAppliesTo { get; set; }

    public virtual ICollection<Content> Contents { get; set; } = new List<Content>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
