using System.Collections.Generic;

namespace ProCoder.Models;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalProblems { get; set; }
    public int MonthlySubmissions { get; set; }
    public List<RoleDistributionItem> RoleDistribution { get; set; } = new List<RoleDistributionItem>();
}

public class RoleDistributionItem
{
    public string Role { get; set; } = null!;
    public int Count { get; set; }
} 