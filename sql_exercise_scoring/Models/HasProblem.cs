namespace sql_exercise_scoring.Models
{
    public class HasProblem
    {
        public int HasProblemId { get; set; }
        public int ContestId { get; set; }
        public int ProblemId { get; set; }
        public int ProblemOrder { get; set; }
        public int PointProblem { get; set; }

        public Contest Contest { get; set; }
        public Problem Problem { get; set; }
    }
}
