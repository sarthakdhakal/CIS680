using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace NewDotnet.Models;

public partial class Content
{
    public int ContentId { get; set; }

    public int SectionId { get; set; }

    public string ContentData { get; set; } = null!;

    public string? ContentTitle { get; set; }

    public string HeaderImage { get; set; } = null!;

    public string? Comment { get; set; }

    public virtual Section Section { get; set; } = null!;

    public virtual ICollection<Flag> Flags { get; set; } = new List<Flag>();

}

