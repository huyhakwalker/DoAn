using System;
using System.Collections.Generic;

namespace sql_exercise_scoring.Models;

public partial class TestRun
{
    public int TestRunId { get; set; }

    public int SubmissionId { get; set; }

    public int TestCaseId { get; set; }

    public int TimeDuration { get; set; }

    public int MemorySize { get; set; }

    public string TestOutput { get; set; } = null!;

    public string Result { get; set; } = null!;

    public string? CheckerLog { get; set; }

    public virtual Submission Submission { get; set; } = null!;

    public virtual TestCase TestCase { get; set; } = null!;
}
