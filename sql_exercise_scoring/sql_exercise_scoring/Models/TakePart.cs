using System;
using System.Collections.Generic;

namespace sql_exercise_scoring.Models;

public partial class TakePart
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

    public virtual Participation Participation { get; set; } = null!;

    public virtual Problem Problem { get; set; } = null!;

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
