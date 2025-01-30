using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;

namespace ProCoder.Controllers
{
    public class ProblemsController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public ProblemsController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var problems = await _context.Problems
                .Include(p => p.ProblemThemes)
                    .ThenInclude(pt => pt.Theme)
                .Include(p => p.Submissions)
                .Include(p => p.Coder)
                .OrderBy(p => p.ProblemId)
                .ToListAsync();
            return View(problems);
        }

        public async Task<IActionResult> Details(int id)
        {
            var problem = await _context.Problems
                .Include(p => p.ProblemThemes)
                    .ThenInclude(pt => pt.Theme)
                .Include(p => p.Submissions)
                .Include(p => p.Coder)
                .Include(p => p.DatabaseSchema)
                .FirstOrDefaultAsync(p => p.ProblemId == id);

            if (problem == null)
            {
                return NotFound();
            }

            return View(problem);
        }
    }
}
