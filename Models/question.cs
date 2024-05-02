using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int SectionId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string? QuestionAnswers { get; set; }

    public virtual Section Section { get; set; } = null!;
}
