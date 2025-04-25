using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProCoder.Controllers
{
    public class SubmissionsController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public SubmissionsController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        // GET: Submissions
        public async Task<IActionResult> Index()
        {
            var submissions = await _context.Submissions
                .Include(s => s.Problem)
                .Include(s => s.Coder)
                .OrderByDescending(s => s.SubmitTime)
                .ToListAsync();

            return View(submissions);
        }

        // GET: Submissions/Create
        public async Task<IActionResult> Create()
        {
            // Get list of problems for dropdown
            ViewData["ProblemId"] = new SelectList(await _context.Problems
                .OrderBy(p => p.ProblemName)
                .ToListAsync(), "ProblemId", "ProblemName");

            // Get list of coders for dropdown
            ViewData["CoderId"] = new SelectList(await _context.Coders
                .OrderBy(c => c.CoderName)
                .ToListAsync(), "CoderId", "CoderName");

            // Optional: Get list of TakeParts for dropdown (if needed)
            ViewData["TakePartId"] = new SelectList(await _context.TakeParts
                .Include(tp => tp.Problem)
                .Include(tp => tp.Participation)
                .ThenInclude(p => p.Coder)
                .Select(tp => new
                {
                    tp.TakePartId,
                    Description = $"{tp.Problem.ProblemName} - {tp.Participation.Coder.CoderName}"
                })
                .ToListAsync(), "TakePartId", "Description");

            return View();
        }

        // POST: Submissions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProblemId,CoderId,TakePartId,SubmitCode,SubmissionStatus,Score,ExecutionTime,ErrorMessage")] Submission submission)
        {
            if (ModelState.IsValid)
            {
                // Set default values
                submission.SubmitTime = DateTime.Now;
                submission.CreatedAt = DateTime.Now;
                
                _context.Add(submission);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // If we got this far, something failed, redisplay form
            ViewData["ProblemId"] = new SelectList(await _context.Problems
                .OrderBy(p => p.ProblemName)
                .ToListAsync(), "ProblemId", "ProblemName", submission.ProblemId);
                
            ViewData["CoderId"] = new SelectList(await _context.Coders
                .OrderBy(c => c.CoderName)
                .ToListAsync(), "CoderId", "CoderName", submission.CoderId);
                
            ViewData["TakePartId"] = new SelectList(await _context.TakeParts
                .Include(tp => tp.Problem)
                .Include(tp => tp.Participation)
                .ThenInclude(p => p.Coder)
                .Select(tp => new
                {
                    tp.TakePartId,
                    Description = $"{tp.Problem.ProblemName} - {tp.Participation.Coder.CoderName}"
                })
                .ToListAsync(), "TakePartId", "Description", submission.TakePartId);
                
            return View(submission);
        }
    }
} 