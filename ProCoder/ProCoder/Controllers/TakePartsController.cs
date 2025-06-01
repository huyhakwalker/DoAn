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
    [Authorize]
    public class TakePartsController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public TakePartsController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        // GET: TakeParts
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var takeParts = await _context.TakeParts
                .Include(t => t.Participation)
                .Include(t => t.Participation.Coder)
                .Include(t => t.Participation.Contest)
                .Include(t => t.Problem)
                .ToListAsync();
            
            return View(takeParts);
        }

        // GET: TakeParts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var takePart = await _context.TakeParts
                .Include(t => t.Participation)
                .Include(t => t.Participation.Coder)
                .Include(t => t.Participation.Contest)
                .Include(t => t.Problem)
                .FirstOrDefaultAsync(m => m.TakePartId == id);
            
            if (takePart == null)
            {
                return NotFound();
            }

            // Check if user is admin or the participant
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CreatedBy == userId);
            
            if (coder == null || (takePart.Participation.CoderId != coder.CoderId && !User.IsInRole("Admin")))
            {
                return Forbid();
            }

            return View(takePart);
        }

        // GET: TakeParts/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["ParticipationId"] = new SelectList(_context.Participations
                .Include(p => p.Coder)
                .Include(p => p.Contest)
                .Select(p => new 
                {
                    p.ParticipationId,
                    DisplayName = $"{p.Coder.CoderName} - {p.Contest.ContestName}" 
                }), "ParticipationId", "DisplayName");
            ViewData["ProblemId"] = new SelectList(_context.Problems, "ProblemId", "ProblemName");
            return View();
        }

        // POST: TakeParts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ParticipationId,ProblemId,PointWon")] TakePart takePart)
        {
            if (ModelState.IsValid)
            {
                // Check if this participation already has a record for this problem
                var existingTakePart = await _context.TakeParts
                    .FirstOrDefaultAsync(t => t.ParticipationId == takePart.ParticipationId && t.ProblemId == takePart.ProblemId);
                
                if (existingTakePart != null)
                {
                    ModelState.AddModelError("", "This participation already has a record for this problem.");
                    ViewData["ParticipationId"] = new SelectList(_context.Participations
                        .Include(p => p.Coder)
                        .Include(p => p.Contest)
                        .Select(p => new 
                        {
                            p.ParticipationId,
                            DisplayName = $"{p.Coder.CoderName} - {p.Contest.ContestName}" 
                        }), "ParticipationId", "DisplayName", takePart.ParticipationId);
                    ViewData["ProblemId"] = new SelectList(_context.Problems, "ProblemId", "ProblemName", takePart.ProblemId);
                    return View(takePart);
                }
                
                // Set default values
                takePart.TimeSolved = DateTime.Now;
                takePart.SubmissionCount = 0;
                
                _context.Add(takePart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ParticipationId"] = new SelectList(_context.Participations
                .Include(p => p.Coder)
                .Include(p => p.Contest)
                .Select(p => new 
                {
                    p.ParticipationId,
                    DisplayName = $"{p.Coder.CoderName} - {p.Contest.ContestName}" 
                }), "ParticipationId", "DisplayName", takePart.ParticipationId);
            ViewData["ProblemId"] = new SelectList(_context.Problems, "ProblemId", "ProblemName", takePart.ProblemId);
            return View(takePart);
        }

        // GET: TakeParts/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var takePart = await _context.TakeParts.FindAsync(id);
            
            if (takePart == null)
            {
                return NotFound();
            }
            
            ViewData["ParticipationId"] = new SelectList(_context.Participations
                .Include(p => p.Coder)
                .Include(p => p.Contest)
                .Select(p => new 
                {
                    p.ParticipationId,
                    DisplayName = $"{p.Coder.CoderName} - {p.Contest.ContestName}" 
                }), "ParticipationId", "DisplayName", takePart.ParticipationId);
            ViewData["ProblemId"] = new SelectList(_context.Problems, "ProblemId", "ProblemName", takePart.ProblemId);
            return View(takePart);
        }

        // POST: TakeParts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("TakePartId,ParticipationId,ProblemId,TimeSolved,PointWon,SubmissionCount")] TakePart takePart)
        {
            if (id != takePart.TakePartId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if there's another record with the same participation and problem but different ID
                    var duplicate = await _context.TakeParts
                        .Where(t => t.ParticipationId == takePart.ParticipationId && t.ProblemId == takePart.ProblemId && t.TakePartId != takePart.TakePartId)
                        .FirstOrDefaultAsync();
                        
                    if (duplicate != null)
                    {
                        ModelState.AddModelError("", "This participation already has another record for this problem.");
                        ViewData["ParticipationId"] = new SelectList(_context.Participations
                            .Include(p => p.Coder)
                            .Include(p => p.Contest)
                            .Select(p => new 
                            {
                                p.ParticipationId,
                                DisplayName = $"{p.Coder.CoderName} - {p.Contest.ContestName}" 
                            }), "ParticipationId", "DisplayName", takePart.ParticipationId);
                        ViewData["ProblemId"] = new SelectList(_context.Problems, "ProblemId", "ProblemName", takePart.ProblemId);
                        return View(takePart);
                    }
                    
                    _context.Update(takePart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TakePartExists(takePart.TakePartId))
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
            
            ViewData["ParticipationId"] = new SelectList(_context.Participations
                .Include(p => p.Coder)
                .Include(p => p.Contest)
                .Select(p => new 
                {
                    p.ParticipationId,
                    DisplayName = $"{p.Coder.CoderName} - {p.Contest.ContestName}" 
                }), "ParticipationId", "DisplayName", takePart.ParticipationId);
            ViewData["ProblemId"] = new SelectList(_context.Problems, "ProblemId", "ProblemName", takePart.ProblemId);
            return View(takePart);
        }

        // GET: TakeParts/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var takePart = await _context.TakeParts
                .Include(t => t.Participation)
                .Include(t => t.Participation.Coder)
                .Include(t => t.Participation.Contest)
                .Include(t => t.Problem)
                .FirstOrDefaultAsync(m => m.TakePartId == id);
            
            if (takePart == null)
            {
                return NotFound();
            }

            return View(takePart);
        }

        // POST: TakeParts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var takePart = await _context.TakeParts.FindAsync(id);
            if (takePart != null)
            {
                _context.TakeParts.Remove(takePart);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: TakeParts/ContestLeaderboard/5
        public async Task<IActionResult> ContestLeaderboard(int? id)
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

            var leaderboard = await _context.Participations
                .Include(p => p.Coder)
                .Include(p => p.TakeParts)
                .Where(p => p.ContestId == id)
                .OrderByDescending(p => p.PointScore)
                .ThenBy(p => p.TimeScore)
                .ToListAsync();

            ViewData["Contest"] = contest;
            return View(leaderboard);
        }

        // GET: TakeParts/UserRanking
        public async Task<IActionResult> UserRanking()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CreatedBy == userId);
            
            if (coder == null)
            {
                return NotFound();
            }

            var userRankings = await _context.Participations
                .Include(p => p.Contest)
                .Include(p => p.TakeParts)
                .Where(p => p.CoderId == coder.CoderId)
                .OrderByDescending(p => p.Contest.EndTime)
                .ToListAsync();

            return View(userRankings);
        }

        private bool TakePartExists(int id)
        {
            return _context.TakeParts.Any(e => e.TakePartId == id);
        }
    }
} 