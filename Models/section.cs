using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class Section
{
    public int SectionId { get; set; }

    public string SectionTitle { get; set; } = null!;

    public string? Comment { get; set; }

    public virtual ICollection<AccountQuiz> AccountQuizzes { get; set; } = new List<AccountQuiz>();

    public virtual ICollection<Content> Contents { get; set; } = new List<Content>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Flag> Flags { get; set; } = new List<Flag>();
}
