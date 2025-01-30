using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class TestCase
{
    public int TestCaseId { get; set; }

    public int ProblemId { get; set; }

    public string TestQuery { get; set; } = null!;

    public string ExpectedResult { get; set; } = null!;

    public bool IsHidden { get; set; }

    public int OrderNumber { get; set; }

    public int Score { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Problem Problem { get; set; } = null!;

    public virtual ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
}
