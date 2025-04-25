using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize]
    public class ProblemsAdminController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public ProblemsAdminController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var problems = await _context.Problems
                .Include(p => p.Theme)
                .Include(p => p.Coder)
                .Include(p => p.DatabaseSchema)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Lấy số lượng test case cho mỗi bài toán
            var problemIds = problems.Select(p => p.ProblemId).ToList();
            var testCaseCounts = await _context.TestCases
                .Where(t => problemIds.Contains(t.ProblemId))
                .GroupBy(t => t.ProblemId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
                
            ViewBag.TestCaseCounts = testCaseCounts;

            return View(problems);
        }

        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            // Tạo SelectList với option mặc định
            var themes = await _context.Themes
                .OrderBy(t => t.ThemeName)
                .ToListAsync();
            
            var schemas = await _context.DatabaseSchemas
                .OrderBy(d => d.SchemaName)
                .ToListAsync();

            ViewBag.Themes = new SelectList(themes, "ThemeId", "ThemeName");
            ViewBag.DatabaseSchemas = new SelectList(schemas, "DatabaseSchemaId", "SchemaName");

            return View(new Problem { Published = false });
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Problem problem)
        {
            try
            {
                var coderName = User.Identity?.Name;
                var coder = await _context.Coders
                    .FirstOrDefaultAsync(c => c.CoderName == coderName);
                
                if (coder == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Trim các giá trị string
                problem.ProblemCode = problem.ProblemCode?.Trim();
                problem.ProblemName = problem.ProblemName?.Trim();
                problem.ProblemDescription = problem.ProblemDescription?.Trim();
                problem.ProblemExplanation = problem.ProblemExplanation?.Trim();

                problem.CoderId = coder.CoderId;
                problem.CreatedAt = DateTime.UtcNow;
                problem.UpdatedAt = DateTime.UtcNow;

                // Kiểm tra Theme và DatabaseSchema tồn tại
                var theme = await _context.Themes.FindAsync(problem.ThemeId);
                var schema = await _context.DatabaseSchemas.FindAsync(problem.DatabaseSchemaId);

                if (theme == null)
                {
                    TempData["ErrorMessage"] = "Dạng bài không tồn tại";
                    return View(problem);
                }

                if (schema == null)
                {
                    TempData["ErrorMessage"] = "Database Schema không tồn tại";
                    return View(problem);
                }

                _context.Add(problem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tạo bài tập thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tạo bài tập: {ex.Message}";
            }

            // Load lại dropdowns nếu có lỗi
            ViewBag.Themes = new SelectList(
                await _context.Themes.OrderBy(t => t.ThemeName).ToListAsync(),
                "ThemeId",
                "ThemeName",
                problem.ThemeId
            );
            
            ViewBag.DatabaseSchemas = new SelectList(
                await _context.DatabaseSchemas.OrderBy(d => d.SchemaName).ToListAsync(),
                "DatabaseSchemaId",
                "SchemaName",
                problem.DatabaseSchemaId
            );

            return View(problem);
        }

        [HttpGet]
        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var problem = await _context.Problems.FirstOrDefaultAsync(p => p.ProblemId == id);
            if (problem == null)
            {
                return NotFound();
            }

            ViewBag.Themes = new SelectList(
                await _context.Themes.OrderBy(t => t.ThemeName).ToListAsync(),
                "ThemeId",
                "ThemeName",
                problem.ThemeId
            );
            
            ViewBag.DatabaseSchemas = new SelectList(
                await _context.DatabaseSchemas.OrderBy(d => d.SchemaName).ToListAsync(),
                "DatabaseSchemaId",
                "SchemaName",
                problem.DatabaseSchemaId
            );

            ViewBag.Coders = new SelectList(
                await _context.Coders.OrderBy(c => c.CoderName).ToListAsync(),
                "CoderId",
                "CoderName",
                problem.CoderId
            );

            return View(problem);
        }

        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Problem problem)
        {
            if (id != problem.ProblemId)
            {
                return NotFound();
            }

            try
            {
                var existingProblem = await _context.Problems.FirstOrDefaultAsync(p => p.ProblemId == id);
                if (existingProblem == null)
                {
                    return NotFound();
                }

                // Cập nhật các trường
                existingProblem.ProblemCode = problem.ProblemCode?.Trim();
                existingProblem.ProblemName = problem.ProblemName?.Trim();
                existingProblem.ProblemDescription = problem.ProblemDescription?.Trim();
                existingProblem.ProblemExplanation = problem.ProblemExplanation?.Trim();
                existingProblem.Published = problem.Published;
                existingProblem.ThemeId = problem.ThemeId;
                existingProblem.DatabaseSchemaId = problem.DatabaseSchemaId;
                existingProblem.CoderId = problem.CoderId;
                existingProblem.UpdatedAt = DateTime.UtcNow;

                _context.Update(existingProblem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật bài tập thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
            }

            // Load lại dropdowns nếu có lỗi
            ViewBag.Themes = new SelectList(
                await _context.Themes.OrderBy(t => t.ThemeName).ToListAsync(),
                "ThemeId",
                "ThemeName",
                problem.ThemeId
            );
            
            ViewBag.DatabaseSchemas = new SelectList(
                await _context.DatabaseSchemas.OrderBy(d => d.SchemaName).ToListAsync(),
                "DatabaseSchemaId",
                "SchemaName",
                problem.DatabaseSchemaId
            );

            ViewBag.Coders = new SelectList(
                await _context.Coders.OrderBy(c => c.CoderName).ToListAsync(),
                "CoderId",
                "CoderName",
                problem.CoderId
            );

            return View(problem);
        }

        [HttpGet]
        [Route("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var problem = await _context.Problems
                .Include(p => p.Theme)
                .Include(p => p.Coder)
                .Include(p => p.DatabaseSchema)
                .Include(p => p.TestCases)
                    .ThenInclude(tc => tc.InitData)
                .FirstOrDefaultAsync(p => p.ProblemId == id);

            if (problem == null)
            {
                return NotFound();
            }
            return View(problem);
        }

        [HttpPost]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var problem = await _context.Problems.FindAsync(id);
            if (problem == null)
            {
                return NotFound();
            }

            try
            {
                // Kiểm tra xem bài toán có test case không
                var hasTestCases = await _context.TestCases.AnyAsync(t => t.ProblemId == id);
                if (hasTestCases)
                {
                    // Xóa tất cả test case liên quan
                    var testCases = await _context.TestCases.Where(t => t.ProblemId == id).ToListAsync();
                    _context.TestCases.RemoveRange(testCases);
                }

                // Kiểm tra xem bài toán có trong contest không
                var hasContests = await _context.HasProblems.AnyAsync(hp => hp.ProblemId == id);
                if (hasContests)
                {
                    // Xóa tất cả liên kết với contest
                    var contestLinks = await _context.HasProblems.Where(hp => hp.ProblemId == id).ToListAsync();
                    _context.HasProblems.RemoveRange(contestLinks);
                }

                _context.Problems.Remove(problem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa bài tập thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa bài tập: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}