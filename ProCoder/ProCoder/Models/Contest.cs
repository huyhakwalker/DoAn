using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class Contest
{
    public int ContestId { get; set; }

    public int CoderId { get; set; }

    public string ContestName { get; set; } = null!;

    public string ContestDescription { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool Published { get; set; }

    public string StatusContest { get; set; } = null!;

    public int Duration { get; set; }

    public virtual Coder Coder { get; set; } = null!;

    public virtual ICollection<HasProblem> HasProblems { get; set; } = new List<HasProblem>();

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();
}
