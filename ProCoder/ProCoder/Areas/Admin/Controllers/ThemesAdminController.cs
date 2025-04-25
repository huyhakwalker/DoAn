using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class ThemesAdminController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public ThemesAdminController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        // GET: Admin/ThemesAdmin
        [Route("")]
        [Route("Index")]
        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            var themes = from t in _context.Themes
                         select t;

            if (!string.IsNullOrEmpty(searchString))
            {
                themes = themes.Where(t => t.ThemeName.Contains(searchString));
            }

            int pageSize = 10;
            return View(await themes.OrderBy(t => t.ThemeOrder)
                                  .Include(t => t.Problems)
                                  .ToListAsync());
        }

        // GET: Admin/ThemesAdmin/Create
        [Route("Create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ThemesAdmin/Create
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ThemeName,ThemeOrder")] Theme theme)
        {
            if (ModelState.IsValid)
            {
                _context.Add(theme);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm dạng bài thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(theme);
        }

        // GET: Admin/ThemesAdmin/Edit/5
        [Route("Edit/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var theme = await _context.Themes.FindAsync(id);
            if (theme == null)
            {
                return NotFound();
            }
            return View(theme);
        }

        // POST: Admin/ThemesAdmin/Edit/5
        [Route("Edit/{id:int}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ThemeId,ThemeName,ThemeOrder")] Theme theme)
        {
            if (id != theme.ThemeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(theme);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật dạng bài thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThemeExists(theme.ThemeId))
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
            return View(theme);
        }

        // GET: Admin/ThemesAdmin/Delete/5
        [Route("Delete/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var theme = await _context.Themes
                .Include(t => t.Problems)
                .FirstOrDefaultAsync(t => t.ThemeId == id);

            if (theme == null)
            {
                return NotFound();
            }

            return View(theme);
        }

        // POST: Admin/ThemesAdmin/Delete/5
        [Route("Delete/{id:int}")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var theme = await _context.Themes
                    .Include(t => t.Problems)  // Include Problems để xóa cascade
                    .FirstOrDefaultAsync(t => t.ThemeId == id);

                if (theme == null)
                {
                    return NotFound();
                }

                // Xóa tất cả các Problems liên quan đến Theme này
                foreach (var problem in theme.Problems.ToList())
                {
                    // Xóa các TestCases liên quan
                    var testCases = _context.TestCases.Where(tc => tc.ProblemId == problem.ProblemId);
                    _context.TestCases.RemoveRange(testCases);

                    // Xóa các Submissions liên quan
                    var submissions = _context.Submissions.Where(s => s.ProblemId == problem.ProblemId);
                    _context.Submissions.RemoveRange(submissions);

                    // Xóa các HasProblems liên quan
                    var hasProblems = _context.HasProblems.Where(hp => hp.ProblemId == problem.ProblemId);
                    _context.HasProblems.RemoveRange(hasProblems);

                    // Xóa các TakeParts liên quan
                    var takeParts = _context.TakeParts.Where(tp => tp.ProblemId == problem.ProblemId);
                    _context.TakeParts.RemoveRange(takeParts);

                    // Xóa các Favourites liên quan
                    var favourites = _context.Favourites.Where(f => f.ProblemId == problem.ProblemId);
                    _context.Favourites.RemoveRange(favourites);

                    // Xóa Problem
                    _context.Problems.Remove(problem);
                }

                // Cuối cùng xóa Theme
                _context.Themes.Remove(theme);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa dạng bài và các bài tập liên quan thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi xóa dạng bài: {ex.Message}. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ThemeExists(int id)
        {
            return _context.Themes.Any(e => e.ThemeId == id);
        }
    }
}
