namespace sql_exercise_scoring.Models
{
    public class Favourite
    {
        public int CoderId { get; set; }
        public int ProblemId { get; set; }
        public string? Note { get; set; }

        public Coder Coder { get; set; }
        public Problem Problem { get; set; }
    }
}
