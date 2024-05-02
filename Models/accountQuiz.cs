using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class AccountQuiz
{
    public string StarId { get; set; } = null!;

    public int SectionId { get; set; }

    public DateTime TimeCompleted { get; set; }

    public bool AccountPassed { get; set; }

    public virtual Section Section { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;
}
