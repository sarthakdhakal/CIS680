using System;
using System.Collections.Generic;

namespace NewDotnet.Models;

public partial class Account
{
    public string StarId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;


    public virtual ICollection<AccountQuiz> AccountQuizzes { get; set; } = new List<AccountQuiz>();

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<Flag> Flags { get; set; } = new List<Flag>();
}
