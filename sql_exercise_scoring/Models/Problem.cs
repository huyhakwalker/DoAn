namespace sql_exercise_scoring.Models
{
    public class Problem
    {
        public int ProblemId { get; set; }
        public string ProblemCode { get; set; } = string.Empty;
        public string ProblemName { get; set; } = string.Empty;
        public string ProblemContent { get; set; } = string.Empty;
        public string? ProblemExplanation { get; set; }
        public string TestType { get; set; } = string.Empty;
        public string? TestCode { get; set; }
        public string? TestProgCompilations { get; set; }
        public bool Published { get; set; }
        public int CoderId { get; set; }
        public int EngineId { get; set; }
        public int? ReviewCoderId { get; set; }

        public Coder Coder { get; set; }
        public SqlEngine Engine { get; set; }

        public ICollection<Favourite> Favourites { get; set; }
        public ICollection<Solved> Solveds { get; set; }
        public ICollection<TestCase> TestCases { get; set; }
        public ICollection<Submission> Submissions { get; set; }
        public ICollection<HasProblem> HasProblems { get; set; }
        public ICollection<ProblemTheme> ProblemThemes { get; set; }
        public ICollection<DatabaseSchema> DatabaseSchemas { get; set; }
        public ICollection<TakePart> TakeParts { get; set; }
    }
}
