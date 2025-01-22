namespace sql_exercise_scoring.Models
{
    public class TakePart
    {
        public int TakePartId { get; set; }
        public int ParticipationId { get; set; }
        public int ProblemId { get; set; }
        public DateTime TimeSolved { get; set; }
        public int PointWon { get; set; }
        public int MaxPoint { get; set; }
        public int SubmissionCount { get; set; }
        public string? SubmitMac { get; set; }
        public DateTime? FrozenTime { get; set; }

        public Participation Participation { get; set; }
        public Problem Problem { get; set; }
        public ICollection<Submission> Submissions { get; set; }
    }
}
