namespace sql_exercise_scoring.Models
{
    public class Submission
    {
        public int SubmissionId { get; set; }
        public int EngineId { get; set; }
        public int TakePartId { get; set; }
        public int ProblemId { get; set; }
        public int CoderId { get; set; }
        public DateTime SubmitTime { get; set; }
        public string SubmitCode { get; set; } = string.Empty;
        public string SubmissionStatus { get; set; } = string.Empty;
        public int SubmitLineCount { get; set; }
        public int TestRunCount { get; set; }
        public string? TestResult { get; set; }
        public int MaxMemorySize { get; set; }
        public int MaxTimeDuration { get; set; }
        public int SubmitMinute { get; set; }
        public SqlEngine Engine { get; set; }
        public TakePart TakePart { get; set; }
        public Problem Problem { get; set; }
        public Coder Coder { get; set; }
        public ICollection<TestRun> TestRuns { get; set; }
    }
}
