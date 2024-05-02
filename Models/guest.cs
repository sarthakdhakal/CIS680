using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class Guest
{
    public string GuestId { get; set; } = null!;

    public int? AssignedPlaylist { get; set; }

    public int CurrentPosition { get; set; }

    public DateTime LastAction { get; set; }

    public virtual Playlist1? AssignedPlaylistNavigation { get; set; }
}
