using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize]
    public class ProblemsAdminController : Controller
    {
        private readonly SqlExerciseScoringContext _context;
        private readonly ILogger<ProblemsAdminController> _logger;

        public ProblemsAdminController(SqlExerciseScoringContext context, ILogger<ProblemsAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = null)
        {
            var query = _context.Problems
                .Include(p => p.Theme)
                .Include(p => p.Coder)
                .Include(p => p.DatabaseSchema)
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();

            // Tìm kiếm theo tên bài toán hoặc người tạo
            if (!string.IsNullOrEmpty(search))
            {
                string searchLower = search.Trim().ToLower();
                query = query.Where(p => p.ProblemName.ToLower().Contains(searchLower)
                                     || p.Coder.CoderName.ToLower().Contains(searchLower));
            }

            // Tính toán số lượng trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Đảm bảo trang hiện tại hợp lệ
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            // Lấy dữ liệu cho trang hiện tại
            var problems = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lấy số lượng test case cho mỗi bài toán
            var problemIds = problems.Select(p => p.ProblemId).ToList();
            var testCaseCounts = await _context.TestCases
                .Where(t => problemIds.Contains(t.ProblemId))
                .GroupBy(t => t.ProblemId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
                
            ViewBag.TestCaseCounts = testCaseCounts;

            // Truyền thông tin phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchString = search;
            ViewBag.TotalItems = totalItems;

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
        public async Task<IActionResult> Create(Problem problem, string AnswerQuery)
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
                problem.ProblemCode = problem.ProblemCode?.Trim().ToLower();
                problem.ProblemName = problem.ProblemName?.Trim();
                problem.ProblemDescription = problem.ProblemDescription?.Trim();
                problem.ProblemExplanation = problem.ProblemExplanation?.Trim();

                // Tạo file câu truy vấn mẫu
                if (!string.IsNullOrEmpty(AnswerQuery))
                {
                    var fileName = $"{problem.ProblemCode}_answer_query.csv";
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "AnswerQuerys");
                    var filePath = Path.Combine(folderPath, fileName);

                    // Đảm bảo thư mục tồn tại
                    Directory.CreateDirectory(folderPath);

                    // Ghi nội dung vào file
                    await System.IO.File.WriteAllTextAsync(filePath, AnswerQuery.Trim());

                    // Lưu đường dẫn tương đối vào database
                    problem.AnswerQueryPath = $"/Data/AnswerQuerys/{fileName}";
                }

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

                TempData["SuccessMessageProblem"] = "Tạo bài tập thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageProblem"] = $"Lỗi khi tạo bài tập: {ex.Message}";
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

            // Đọc nội dung file câu truy vấn đáp án
            try
            {
                if (!string.IsNullOrEmpty(problem.AnswerQueryPath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", problem.AnswerQueryPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        ViewBag.AnswerQueryContent = await System.IO.File.ReadAllTextAsync(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading answer query file: {ex.Message}");
            }

            return View(problem);
        }

        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Problem problem, string AnswerQuery)
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

                // Cập nhật file câu truy vấn mẫu nếu có thay đổi
                if (!string.IsNullOrEmpty(AnswerQuery))
                {
                    string filePath;
                    
                    // Nếu đã có đường dẫn file, sử dụng lại
                    if (!string.IsNullOrEmpty(existingProblem.AnswerQueryPath))
                    {
                        filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                            existingProblem.AnswerQueryPath.TrimStart('/'));
                    }
                    // Nếu chưa có, tạo file mới
                    else
                    {
                        var fileName = $"{problem.ProblemCode}_answer_query.csv";
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "AnswerQuerys");
                        
                        // Đảm bảo thư mục tồn tại
                        Directory.CreateDirectory(folderPath);
                        
                        filePath = Path.Combine(folderPath, fileName);
                        existingProblem.AnswerQueryPath = $"/Data/AnswerQuerys/{fileName}";
                    }
                    
                    // Ghi nội dung vào file
                    await System.IO.File.WriteAllTextAsync(filePath, AnswerQuery.Trim());
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

            // Đọc nội dung file câu truy vấn đáp án
            try
            {
                if (!string.IsNullOrEmpty(problem.AnswerQueryPath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", problem.AnswerQueryPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        ViewBag.AnswerQueryContent = await System.IO.File.ReadAllTextAsync(fullPath);
                    }
                }

                // Đọc nội dung file kết quả cho mỗi test case
                foreach (var testCase in problem.TestCases)
                {
                    if (!string.IsNullOrEmpty(testCase.AnswerResultPath))
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", testCase.AnswerResultPath.TrimStart('/'));
                        if (System.IO.File.Exists(fullPath))
                        {
                            testCase.AnswerResultContent = await System.IO.File.ReadAllTextAsync(fullPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading file contents: {ex.Message}");
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
                // Xóa file câu truy vấn mẫu nếu tồn tại
                if (!string.IsNullOrEmpty(problem.AnswerQueryPath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", problem.AnswerQueryPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                // Kiểm tra và xóa test cases
                var testCases = await _context.TestCases
                    .Where(t => t.ProblemId == id)
                    .ToListAsync();

                foreach (var testCase in testCases)
                {
                    // Xóa file kết quả của test case nếu tồn tại
                    if (!string.IsNullOrEmpty(testCase.AnswerResultPath))
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", testCase.AnswerResultPath.TrimStart('/'));
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                }
                
                // Xóa các test cases từ database
                if (testCases.Any())
                {
                    _context.TestCases.RemoveRange(testCases);
                }

                // Xóa liên kết với contests nếu có
                var contestLinks = await _context.HasProblems
                    .Where(hp => hp.ProblemId == id)
                    .ToListAsync();
                if (contestLinks.Any())
                {
                    _context.HasProblems.RemoveRange(contestLinks);
                }

                // Xóa problem
                _context.Problems.Remove(problem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessageProblem"] = "Xóa bài tập và các file liên quan thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi xóa bài tập: {ex.Message}");
                TempData["ErrorMessageProblem"] = $"Lỗi khi xóa bài tập: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}