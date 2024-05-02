using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class CohortPrompt
{
    public int PromptOrder { get; set; }

    public int PlaylistId { get; set; }

    public string PromptText { get; set; } = null!;

    public string PromptOptions { get; set; } = null!;

    public string CohortMap { get; set; } = null!;

    public virtual Playlist1 Playlist { get; set; } = null!;
}
