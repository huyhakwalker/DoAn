namespace ProCoder.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Blog> PinnedBlogs { get; set; }
        public List<TopCoderViewModel> TopCoders { get; set; }
        public List<Problem> RecentProblems { get; set; }
    }
} 