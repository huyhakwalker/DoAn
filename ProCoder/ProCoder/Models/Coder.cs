using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class Coder
{
    public int CoderId { get; set; }

    public string CoderName { get; set; } = null!;

    public string CoderEmail { get; set; } = null!;

    public string? CoderAvatar { get; set; }

    public string Password { get; set; } = null!;

    public bool? ReceiveEmail { get; set; }

    public bool? Gender { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public bool AdminCoder { get; set; }

    public bool ContestSetter { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Contest> Contests { get; set; } = new List<Contest>();

    public virtual ICollection<DatabaseSchema> DatabaseSchemas { get; set; } = new List<DatabaseSchema>();

    public virtual ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<Problem> ProblemsNavigation { get; set; } = new List<Problem>();
}
