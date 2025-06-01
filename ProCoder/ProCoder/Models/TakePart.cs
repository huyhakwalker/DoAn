using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class TakePart
{
    public int TakePartId { get; set; }

    public int ParticipationId { get; set; }

    public int ProblemId { get; set; }

    public DateTime? TimeSolved { get; set; }

    public int PointWon { get; set; }

    public int SubmissionCount { get; set; }

    public virtual Participation Participation { get; set; } = null!;

    public virtual Problem Problem { get; set; } = null!;

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
