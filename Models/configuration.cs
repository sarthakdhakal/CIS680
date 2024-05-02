using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class Configuration
{
    public int OptionId { get; set; }

    public string OptionShortName { get; set; } = null!;

    public string OptionValue { get; set; } = null!;

    public string? OptionAcceptedValues { get; set; }

    public string OptionKind { get; set; } = null!;

    public string? OptionDescription { get; set; }

    public int? OptionAppliesTo { get; set; }

    public virtual Playlist1? OptionAppliesToNavigation { get; set; }
}
