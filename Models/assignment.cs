using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class Assignment
{
    public string StarId { get; set; } = null!;

    public int AssignedPlaylist { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int CurrentProgress { get; set; }

    public int CurrentPosition { get; set; }

    public virtual Account Account { get; set; } = null!;
}
