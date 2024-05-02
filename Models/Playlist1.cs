using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class Playlist1
{
    public int PlaylistId { get; set; }

    public string PlaylistTitle { get; set; } = null!;

    public virtual ICollection<CohortPrompt> CohortPrompts { get; set; } = new List<CohortPrompt>();

    public virtual ICollection<Configuration> Configurations { get; set; } = new List<Configuration>();

    public virtual ICollection<Guest> Guests { get; set; } = new List<Guest>();

    public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
}
