using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class Playlist
{
    public int PlaylistOrder { get; set; }

    public int PlaylistId { get; set; }

    public string ItemType { get; set; } = null!;

    public int ItemId { get; set; }

    public virtual Playlist1 PlaylistNavigation { get; set; } = null!;
}
