using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProCoder.Models;

public partial class TestCase
{
    [Key]
    public int TestCaseId { get; set; }

    public int ProblemId { get; set; }

    public int? InitDataId { get; set; }

    [StringLength(255)]
    public string AnswerResultPath { get; set; } = null!;

    public bool IsHidden { get; set; }

    public int OrderNumber { get; set; }

    public int Score { get; set; }

    [NotMapped]
    public string? AnswerResultContent { get; set; }

    [NotMapped]
    public string? AnswerResult { get; set; }

    [NotMapped]
    public string? SchemaData { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual InitData? InitData { get; set; }

    public virtual Problem? Problem { get; set; }

    public virtual ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
}
