using System;
using System.Collections.Generic;

namespace sql_exercise_scoring.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int BlogId { get; set; }

    public int CoderId { get; set; }

    public DateTime CommentDate { get; set; }

    public string CommentContent { get; set; } = null!;

    public virtual Blog Blog { get; set; } = null!;

    public virtual Coder Coder { get; set; } = null!;
}
