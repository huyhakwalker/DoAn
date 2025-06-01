using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using Microsoft.AspNetCore.Authorization;

namespace ProCoder.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HasProblemsController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public HasProblemsController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        // GET: HasProblems
        public async Task<IActionResult> Index()
        {
            var hasProblems = await _context.HasProblems
                .Include(h => h.Contest)
                .Include(h => h.Problem)
                .ToListAsync();
            
            return View(hasProblems);
        }

        // GET: HasProblems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hasProblem = await _context.HasProblems
                .Include(h => h.Contest)
                .Include(h => h.Problem)
                .FirstOrDefaultAsync(m => m.HasProblemId == id);
            
            if (hasProblem == null)
            {
                return NotFound();
            }

            return View(hasProblem);
        }

        // GET: HasProblems/Create
        public IActionResult Create()
        {
            ViewData["ContestId"] = new SelectList(_context.Contests, "ContestId", "ContestName");
            ViewData["ProblemId"] = new SelectList(_context.Problems, "ProblemId", "ProblemName");
            return View();
        }

        // POST: HasProblems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContestId,ProblemId,Score")] HasProblem hasProblem)
        {
            if (ModelState.IsValid)
            {
                // Check if the problem is already assigned to the contest
                var existingAssignment = await _context.HasProblems
                    .FirstOrDefaultAsync(h => h.ContestId == hasProblem.ContestId && h.ProblemId == hasProblem.ProblemId);
                
                if (existingAssignment != null)
                {
                    ModelState.AddModelError("", "This problem is already assigned to the selected contest.");
                    ViewData["ContestId"] = new SelectList(_context.Contests, "ContestId", "ContestName", hasProblem.ContestId);
                    ViewData["ProblemId"] = new SelectList(_context.Problems, "ProblemId", "ProblemName", hasProblem.ProblemId);
                    return View(hasProblem);
                }
                
                _context.Add(hasProblem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ContestId"] = new SelectList(_context.Contests, "ContestId", "ContestName", hasProblem.ContestId);
            ViewData["ProblemId"] = new SelectList(_context.Problems, "ProblemId", "ProblemName", hasProblem.ProblemId);
            return View(hasProblem);
        }

        // GET: HasProblems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hasProblem = await _context.HasProblems.FindAsync(id);
            
            if (hasProblem == null)
            {
                return NotFound();
            }
            
            ViewData["ContestId"] = new SelectList(_context.Contests, "ContestId", "ContestName", hasProblem.ContestId);
            ViewData["ProblemId"] = new SelectList(_context.Problems, "ProblemId", "ProblemName", hasProblem.ProblemId);
            return View(hasProblem);
        }

        // POST: HasProblems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HasProblemId,ContestId,ProblemId,Score")] HasProblem hasProblem)
        {
            if (id != hasProblem.HasProblemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if there is another problem with the same ContestId and ProblemId but different HasProblemId
                    var duplicate = await _context.HasProblems
                        .Where(h => h.ContestId == hasProblem.ContestId && h.ProblemId == hasProblem.ProblemId && h.HasProblemId != hasProblem.HasProblemId)
                        .FirstOrDefaultAsync();
                        
                    if (duplicate != null)
                    {
                        ModelState.AddModelError("", "This problem is already assigned to the selected contest.");
                        ViewData["ContestId"] = new SelectList(_context.Contests, "ContestId", "ContestName", hasProblem.ContestId);
                        ViewData["ProblemId"] = new SelectList(_context.Problems, "ProblemId", "ProblemName", hasProblem.ProblemId);
                        return View(hasProblem);
                    }
                    
                    _context.Update(hasProblem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HasProblemExists(hasProblem.HasProblemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ContestId"] = new SelectList(_context.Contests, "ContestId", "ContestName", hasProblem.ContestId);
            ViewData["ProblemId"] = new SelectList(_context.Problems, "ProblemId", "ProblemName", hasProblem.ProblemId);
            return View(hasProblem);
        }

        // GET: HasProblems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hasProblem = await _context.HasProblems
                .Include(h => h.Contest)
                .Include(h => h.Problem)
                .FirstOrDefaultAsync(m => m.HasProblemId == id);
            
            if (hasProblem == null)
            {
                return NotFound();
            }

            return View(hasProblem);
        }

        // POST: HasProblems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasProblem = await _context.HasProblems.FindAsync(id);
            if (hasProblem != null)
            {
                _context.HasProblems.Remove(hasProblem);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: HasProblems/ByContest/5
        public async Task<IActionResult> ByContest(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contest = await _context.Contests.FindAsync(id);
            
            if (contest == null)
            {
                return NotFound();
            }

            var hasProblems = await _context.HasProblems
                .Include(h => h.Problem)
                .Where(h => h.ContestId == id)
                .ToListAsync();

            ViewData["Contest"] = contest;
            return View(hasProblems);
        }

        private bool HasProblemExists(int id)
        {
            return _context.HasProblems.Any(e => e.HasProblemId == id);
        }
    }
} 