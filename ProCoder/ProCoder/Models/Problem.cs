using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProCoder.Models;

public partial class Problem
{
    public int ProblemId { get; set; }

    public string ProblemCode { get; set; } = null!;

    public string ProblemName { get; set; } = null!;

    public string ProblemDescription { get; set; } = null!;

    public string? ProblemExplanation { get; set; }

    [StringLength(255)]
    public string AnswerQueryPath { get; set; } = null!;

    public bool Published { get; set; }

    public int CoderId { get; set; }

    public int DatabaseSchemaId { get; set; }

    public int ThemeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Coder Coder { get; set; } = null!;

    public virtual DatabaseSchema DatabaseSchema { get; set; } = null!;

    public virtual ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    public virtual ICollection<HasProblem> HasProblems { get; set; } = new List<HasProblem>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<TakePart> TakeParts { get; set; } = new List<TakePart>();

    public virtual ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();

    public virtual Theme Theme { get; set; } = null!;

    public virtual ICollection<Coder> Coders { get; set; } = new List<Coder>();
}
