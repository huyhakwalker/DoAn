namespace sql_exercise_scoring.Models
{
    public class TestRun
    {
        public int TestRunId { get; set; }
        public int SubmissionId { get; set; }
        public int TestCaseId { get; set; }
        public int TimeDuration { get; set; }
        public int MemorySize { get; set; }
        public string TestOutput { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string? CheckerLog { get; set; }

        public Submission Submission { get; set; }
        public TestCase TestCase { get; set; }
    }
}
