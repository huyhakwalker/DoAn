using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class TestRun
{
    public int TestRunId { get; set; }

    public int SubmissionId { get; set; }

    public int TestCaseId { get; set; }

    public string? ActualOutput { get; set; }

    public bool IsCorrect { get; set; }

    public int? ExecutionTime { get; set; }

    public string? ErrorMessage { get; set; }

    public int Score { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Submission Submission { get; set; } = null!;

    public virtual TestCase TestCase { get; set; } = null!;
}
