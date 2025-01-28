using System;
using System.Collections.Generic;

namespace sql_exercise_scoring.Models;

public partial class TestCase
{
    public int TestCaseId { get; set; }

    public int ProblemId { get; set; }

    public string SampleTest { get; set; } = null!;

    public string? PreTest { get; set; }

    public string ExpectedResult { get; set; } = null!;

    public string CheckerLogic { get; set; } = null!;

    public virtual Problem Problem { get; set; } = null!;

    public virtual ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
}
