using Microsoft.AspNetCore.Mvc;
using ProCoder.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeAdminController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public HomeAdminController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var roleDistribution = new List<RoleDistributionData>();

            // Đếm số Admin
            var adminCount = _context.Coders.Count(c => c.AdminCoder);
            if (adminCount > 0)
                roleDistribution.Add(new RoleDistributionData { Role = "Admin", Count = adminCount });

            // Đếm số Contest Setter (không phải admin)
            var setterCount = _context.Coders.Count(c => c.ContestSetter && !c.AdminCoder);
            if (setterCount > 0)
                roleDistribution.Add(new RoleDistributionData { Role = "Contest Setter", Count = setterCount });

            // Đếm số User thường (không phải admin và không phải contest setter)
            var userCount = _context.Coders.Count(c => !c.AdminCoder && !c.ContestSetter);
            if (userCount > 0)
                roleDistribution.Add(new RoleDistributionData { Role = "User", Count = userCount });

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = _context.Coders.Count(),
                TotalProblems = _context.Problems.Count(),
                MonthlySubmissions = _context.Submissions
                    .Where(s => s.SubmitTime.Month == DateTime.Now.Month && 
                               s.SubmitTime.Year == DateTime.Now.Year)
                    .Count(),
                RoleDistribution = roleDistribution
            };

            return View(viewModel);
        }
    }
}
