namespace sql_exercise_scoring.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int BlogId { get; set; }
        public int CoderId { get; set; }
        public DateTime CommentDate { get; set; }
        public string CommentContent { get; set; } = string.Empty;

        public Blog Blog { get; set; }
        public Coder Coder { get; set; }
    }
}
