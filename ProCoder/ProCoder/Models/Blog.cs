using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class Blog
{
    public int BlogId { get; set; }

    public string BlogTitle { get; set; } = null!;

    public string BlogContent { get; set; } = null!;

    public DateTime BlogDate { get; set; }

    public bool Published { get; set; }

    public int CoderId { get; set; }

    public bool PinHome { get; set; }

    public virtual Coder Coder { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
