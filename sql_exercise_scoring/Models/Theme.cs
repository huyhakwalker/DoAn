namespace sql_exercise_scoring.Models
{
    public class Theme
    {
        public int ThemeId { get; set; }
        public string ThemeName { get; set; } = string.Empty;
        public int ThemeOrder { get; set; }

        public ICollection<ProblemTheme> ProblemThemes { get; set; }
    }
}
