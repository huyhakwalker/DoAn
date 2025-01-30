using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class HasProblem
{
    public int HasProblemId { get; set; }

    public int ContestId { get; set; }

    public int ProblemId { get; set; }

    public int ProblemOrder { get; set; }

    public int PointProblem { get; set; }

    public virtual Contest Contest { get; set; } = null!;

    public virtual Problem Problem { get; set; } = null!;
}
