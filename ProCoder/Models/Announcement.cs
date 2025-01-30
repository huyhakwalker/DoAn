using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class Announcement
{
    public int AnnouncementId { get; set; }

    public int ContestId { get; set; }

    public DateTime AnnounceTime { get; set; }

    public string AnnounceContent { get; set; } = null!;

    public virtual Contest Contest { get; set; } = null!;
}
