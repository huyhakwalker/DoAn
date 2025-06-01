using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class Submission
{
    public int SubmissionId { get; set; }

    public int ProblemId { get; set; }

    public int CoderId { get; set; }

    public int? TakePartId { get; set; }

    public DateTime SubmitTime { get; set; }

    public string SubmitCode { get; set; } = null!;

    public string SubmissionStatus { get; set; } = null!;

    public int Score { get; set; }

    public int? ExecutionTime { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Coder Coder { get; set; } = null!;

    public virtual Problem Problem { get; set; } = null!;

    public virtual TakePart? TakePart { get; set; }

    public virtual ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
}
