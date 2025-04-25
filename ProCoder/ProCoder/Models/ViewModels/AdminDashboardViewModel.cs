using System.Collections.Generic;

namespace ProCoder.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProblems { get; set; }
        public int MonthlySubmissions { get; set; }
        public List<RoleDistributionData> RoleDistribution { get; set; }
    }

    public class RoleDistributionData
    {
        public string Role { get; set; }
        public int Count { get; set; }
    }
}
