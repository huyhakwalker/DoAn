using sql_exercise_scoring.Models;

namespace sql_exercise_scoring.Models
{
    public class Announcement
    {
        public int AnnouncementId { get; set; }
        public int ContestId { get; set; }
        public DateTime AnnounceTime { get; set; }
        public string AnnounceContent { get; set; } = string.Empty;

        public Contest Contest { get; set; }
    }
}
