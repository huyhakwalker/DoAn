using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProCoder.Controllers
{
    public class ContestsController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public ContestsController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        // GET: Contests
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            var currentTime = DateTime.UtcNow;

            // Lấy danh sách các cuộc thi đang diễn ra và sắp diễn ra
            var activeContests = await _context.Contests
                .Where(c => c.EndTime > currentTime && c.Published)
                .OrderBy(c => c.StartTime)
                .Include(c => c.Coder) // Bao gồm thông tin người tạo cuộc thi
                .ToListAsync();

            return View(activeContests); // Trả về danh sách cuộc thi
        }

        // GET: Contests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var contest = await _context.Contests
                .Include(c => c.Coder)
                .Include(c => c.HasProblems)
                    .ThenInclude(hp => hp.Problem)
                .FirstOrDefaultAsync(m => m.ContestId == id);

            if (contest == null)
            {
                return NotFound();
            }

            return View(contest);
        }
    }
}