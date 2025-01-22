namespace sql_exercise_scoring.Models
{
    public class ProblemTheme
    {
        public int ProblemId { get; set; }
        public int ThemeId { get; set; }
        public string? Note { get; set; }

        public Problem Problem { get; set; }
        public Theme Theme { get; set; }
    }
}
