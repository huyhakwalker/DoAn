using System;
using System.Collections.Generic;

namespace sql_exercise_scoring.Models;

public partial class Submission
{
    public int SubmissionId { get; set; }

    public int EngineId { get; set; }

    public int TakePartId { get; set; }

    public int ProblemId { get; set; }

    public int CoderId { get; set; }

    public DateTime SubmitTime { get; set; }

    public string SubmitCode { get; set; } = null!;

    public string SubmissionStatus { get; set; } = null!;

    public int SubmitLineCount { get; set; }

    public int TestRunCount { get; set; }

    public string? TestResult { get; set; }

    public int MaxMemorySize { get; set; }

    public int MaxTimeDuration { get; set; }

    public int SubmitMinute { get; set; }

    public virtual Coder Coder { get; set; } = null!;

    public virtual SqlEngine Engine { get; set; } = null!;

    public virtual Problem Problem { get; set; } = null!;

    public virtual TakePart TakePart { get; set; } = null!;

    public virtual ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
}
