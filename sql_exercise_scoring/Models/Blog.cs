namespace sql_exercise_scoring.Models
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string BlogTitle { get; set; } = string.Empty;
        public string BlogContent { get; set; } = string.Empty;
        public DateTime BlogDate { get; set; }
        public bool Published { get; set; }
        public int CoderId { get; set; }
        public bool PinHome { get; set; }

        public Coder Coder { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
