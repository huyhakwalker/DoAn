using System;
using System.Collections.Generic;

namespace sql_exercise_scoring.Models;

public partial class Favourite
{
    public int CoderId { get; set; }

    public int ProblemId { get; set; }

    public string? Note { get; set; }

    public virtual Coder Coder { get; set; } = null!;

    public virtual Problem Problem { get; set; } = null!;
}
