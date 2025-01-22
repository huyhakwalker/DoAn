namespace sql_exercise_scoring.Models
{
    public class Contest
    {
        public int ContestId { get; set; }
        public int CoderId { get; set; }
        public string ContestName { get; set; } = string.Empty;
        public string ContestDescription { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string RuleType { get; set; } = string.Empty;
        public int FailedPenalty { get; set; }
        public bool Published { get; set; }
        public string StatusContest { get; set; } = string.Empty;
        public int Duration { get; set; }
        public bool RankingFinished { get; set; }
        public DateTime? FrozenTime { get; set; }

        public Coder Coder { get; set; }
        public ICollection<Participation> Participations { get; set; }
        public ICollection<Announcement> Announcements { get; set; }
        public ICollection<HasProblem> HasProblems { get; set; }
    }
}
