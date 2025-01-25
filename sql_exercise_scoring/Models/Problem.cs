using System;
using System.Collections.Generic;

namespace sql_exercise_scoring.Models;

public partial class Problem
{
    public int ProblemId { get; set; }

    public string ProblemCode { get; set; } = null!;

    public string ProblemName { get; set; } = null!;

    public string ProblemContent { get; set; } = null!;

    public string? ProblemExplanation { get; set; }

    public string TestType { get; set; } = null!;

    public string? TestCode { get; set; }

    public string? TestProgCompilations { get; set; }

    public bool Published { get; set; }

    public int CoderId { get; set; }

    public int EngineId { get; set; }

    public int? ReviewCoderId { get; set; }

    public virtual Coder Coder { get; set; } = null!;

    public virtual ICollection<DatabaseSchema> DatabaseSchemas { get; set; } = new List<DatabaseSchema>();

    public virtual SqlEngine Engine { get; set; } = null!;

    public virtual ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    public virtual ICollection<HasProblem> HasProblems { get; set; } = new List<HasProblem>();

    public virtual ICollection<ProblemTheme> ProblemThemes { get; set; } = new List<ProblemTheme>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<TakePart> TakeParts { get; set; } = new List<TakePart>();

    public virtual ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();

    public virtual ICollection<Coder> Coders { get; set; } = new List<Coder>();
}
