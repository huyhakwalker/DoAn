using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class Participation
{
    public int ParticipationId { get; set; }

    public int CoderId { get; set; }

    public int ContestId { get; set; }

    public DateTime RegisterTime { get; set; }

    public int PointScore { get; set; }

    public int TimeScore { get; set; }

    public int Ranking { get; set; }

    public int SolvedCount { get; set; }

    public virtual Coder Coder { get; set; } = null!;

    public virtual Contest Contest { get; set; } = null!;

    public virtual ICollection<TakePart> TakeParts { get; set; } = new List<TakePart>();
}
