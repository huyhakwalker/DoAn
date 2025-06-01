using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class Solved
{
    public int CoderId { get; set; }

    public int ProblemId { get; set; }

    public virtual Coder Coder { get; set; } = null!;

    public virtual Problem Problem { get; set; } = null!;
} 