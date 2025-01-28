using System;
using System.Collections.Generic;

namespace sql_exercise_scoring.Models;

public partial class ProblemTheme
{
    public int ProblemId { get; set; }

    public int ThemeId { get; set; }

    public string? Note { get; set; }

    public virtual Problem Problem { get; set; } = null!;

    public virtual Theme Theme { get; set; } = null!;
}
