using System;
using System.Collections.Generic;

namespace sql_exercise_scoring.Models;

public partial class Coder
{
    public int CoderId { get; set; }

    public string CoderName { get; set; } = null!;

    public string CoderEmail { get; set; } = null!;

    public string? CoderAvatar { get; set; }

    public string? DescriptionCoder { get; set; }

    public string PwdMd5 { get; set; } = null!;

    public string? SaltMd5 { get; set; }

    public bool AdminCoder { get; set; }

    public bool ContestSetter { get; set; }

    public DateTime RegisterDate { get; set; }

    public string? PwdResetCode { get; set; }

    public DateTime? PwdResetDate { get; set; }

    public bool? ReceiveEmail { get; set; }

    public int? LastCompilerId { get; set; }

    public string? Gender { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Contest> Contests { get; set; } = new List<Contest>();

    public virtual ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<Problem> ProblemsNavigation { get; set; } = new List<Problem>();
}
