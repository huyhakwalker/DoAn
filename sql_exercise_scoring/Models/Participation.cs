namespace sql_exercise_scoring.Models
{
    public class Participation
    {
        public int ParticipationId { get; set; }
        public int CoderId { get; set; }
        public int ContestId { get; set; }
        public DateTime RegisterTime { get; set; }
        public int PointScore { get; set; }
        public int TimeScore { get; set; }
        public int Ranking { get; set; }
        public int SolvedCount { get; set; }
        public string? RegisterMac { get; set; }
        public int? SubRank { get; set; }

        public Coder Coder { get; set; }
        public Contest Contest { get; set; }
        public ICollection<TakePart> TakeParts { get; set; }
    }
}
