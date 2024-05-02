using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class Asset
{
    public int AssetId { get; set; }

    public string AssetName { get; set; } = null!;

    public byte[] AssetData { get; set; } = null!;
}
