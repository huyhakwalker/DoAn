namespace sql_exercise_scoring.Models
{
    public class TestCase
    {
        public int TestCaseId { get; set; }
        public int ProblemId { get; set; }
        public string SampleTest { get; set; } = string.Empty;
        public string? PreTest { get; set; }
        public string ExpectedResult { get; set; } = string.Empty;
        public string CheckerLogic { get; set; } = string.Empty;

        public Problem Problem { get; set; }
        public ICollection<TestRun> TestRuns { get; set; }
    }
}
