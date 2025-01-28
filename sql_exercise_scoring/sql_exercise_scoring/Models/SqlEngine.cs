using System;
using System.Collections.Generic;

namespace sql_exercise_scoring.Models;

public partial class SqlEngine
{
    public int EngineId { get; set; }

    public string EngineName { get; set; } = null!;

    public string Version { get; set; } = null!;

    public string EnginePath { get; set; } = null!;

    public string? EngineOption { get; set; }

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
