using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text;

namespace ProCoder.Controllers
{
    [Authorize]
    public class SubmissionsController : Controller
    {
        private readonly SqlExerciseScoringContext _context;
        private readonly ILogger<SubmissionsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public SubmissionsController(SqlExerciseScoringContext context, ILogger<SubmissionsController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        // GET: Submissions
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            // Hiển thị bài nộp của tất cả người dùng nếu là admin, nếu không chỉ hiển thị của người dùng hiện tại
            var coderId = GetCurrentCoderId();
            if (coderId <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            // Kiểm tra xem người dùng có phải là admin không
            var currentUser = await _context.Coders.FirstOrDefaultAsync(c => c.CoderId == coderId);
            bool isAdmin = currentUser != null && currentUser.AdminCoder;

            var query = _context.Submissions
                .Include(s => s.Coder)
                .Include(s => s.Problem)
                // Nếu là admin, hiển thị tất cả. Nếu không, chỉ hiển thị của người dùng hiện tại
                .Where(s => isAdmin || s.CoderId == coderId)
                .OrderByDescending(s => s.SubmitTime);

            // Tính toán số lượng trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Đảm bảo trang hiện tại hợp lệ
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            // Lấy dữ liệu cho trang hiện tại
            var submissions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền thông tin nếu người dùng là admin
            ViewBag.IsAdmin = isAdmin;
            // Truyền thông tin phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(submissions);
        }

        // GET: Submissions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coderId = GetCurrentCoderId();
            if (coderId <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            var submission = await _context.Submissions
                .Include(s => s.Coder)
                .Include(s => s.Problem)
                .Include(s => s.TestRuns)
                    .ThenInclude(tr => tr.TestCase)
                .FirstOrDefaultAsync(m => m.SubmissionId == id);

            if (submission == null)
            {
                return NotFound();
            }

            // Kiểm tra xem submission có thuộc về người dùng hiện tại
            if (submission.CoderId != coderId)
            {
                // Người dùng không có quyền xem submission này
                return RedirectToAction(nameof(Index));
            }

            // Sort TestRuns by TestCase.OrderNumber for consistent display
            if (submission.TestRuns != null)
            {
                submission.TestRuns = submission.TestRuns
                    .OrderBy(tr => tr.TestCase.OrderNumber)
                    .ToList();
            }

            // Đọc nội dung file kết quả mong đợi cho mỗi test case
            ViewBag.ExpectedOutputs = new Dictionary<int, string>();
            if (submission.TestRuns != null)
            {
                foreach (var testRun in submission.TestRuns)
                {
                    if (!string.IsNullOrEmpty(testRun.TestCase.AnswerResultPath))
                    {
                        try
                        {
                            var filePath = Path.Combine(_environment.WebRootPath, testRun.TestCase.AnswerResultPath.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                            {
                                var content = await System.IO.File.ReadAllTextAsync(filePath);
                                
                                // Nếu nội dung có định dạng CSV, chuyển đổi thành bảng ASCII
                                if (content.Contains(",") && !content.Contains("+--") && !content.Contains("|"))
                                {
                                    content = ConvertCsvToAsciiTable(content);
                                }
                                
                                ViewBag.ExpectedOutputs[testRun.TestRunId] = content;
                            }
                            else
                            {
                                ViewBag.ExpectedOutputs[testRun.TestRunId] = "Không tìm thấy file kết quả";
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Lỗi khi đọc file kết quả mong đợi: {ex.Message}");
                            ViewBag.ExpectedOutputs[testRun.TestRunId] = "Lỗi khi đọc file kết quả";
                        }
                    }
                    else
                    {
                        ViewBag.ExpectedOutputs[testRun.TestRunId] = "Không có đường dẫn file kết quả";
                    }
                    
                    // Kiểm tra xem kết quả thực tế có ở dạng CSV không, nếu có thì chuyển đổi thành bảng ASCII
                    if (!string.IsNullOrEmpty(testRun.ActualOutput) && 
                        testRun.ActualOutput.Contains(",") && 
                        !testRun.ActualOutput.Contains("+--") && 
                        !testRun.ActualOutput.Contains("|"))
                    {
                        testRun.ActualOutput = ConvertCsvToAsciiTable(testRun.ActualOutput);
                    }
                }
            }

            // Nếu submission đã pass và có điểm tối đa, đánh dấu bài tập là đã giải
            if (submission.SubmissionStatus == "Accepted" && submission.Score >= 100)
            {
                // Kiểm tra xem đã tồn tại record Solved chưa
                var existingSolved = await _context.Coders
                    .Where(c => c.CoderId == coderId)
                    .SelectMany(c => c.ProblemsNavigation)
                    .AnyAsync(p => p.ProblemId == submission.ProblemId);
                    
                if (!existingSolved)
                {
                    // Lấy đối tượng Coder và Problem
                    var coder = await _context.Coders
                        .Include(c => c.ProblemsNavigation)
                        .FirstOrDefaultAsync(c => c.CoderId == coderId);
                        
                    var problemToAdd = await _context.Problems
                        .FirstOrDefaultAsync(p => p.ProblemId == submission.ProblemId);
                        
                    if (coder != null && problemToAdd != null)
                    {
                        // Thêm mối quan hệ many-to-many
                        coder.ProblemsNavigation.Add(problemToAdd);
                        _logger.LogInformation($"Marked problem {submission.ProblemId} as solved for coder {coderId}");
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return View(submission);
        }

        // GET: Submissions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coderId = GetCurrentCoderId();
            if (coderId <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            var submission = await _context.Submissions
                .Include(s => s.Coder)
                .Include(s => s.Problem)
                .FirstOrDefaultAsync(m => m.SubmissionId == id);

            if (submission == null)
            {
                return NotFound();
            }

            // Kiểm tra xem submission có thuộc về người dùng hiện tại
            if (submission.CoderId != coderId)
            {
                // Người dùng không có quyền xóa submission này
                return RedirectToAction(nameof(Index));
            }

            return View(submission);
        }

        // POST: Submissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coderId = GetCurrentCoderId();
            if (coderId <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            var submission = await _context.Submissions.FindAsync(id);
            
            // Kiểm tra xem submission có thuộc về người dùng hiện tại
            if (submission.CoderId != coderId)
            {
                // Người dùng không có quyền xóa submission này
                return RedirectToAction(nameof(Index));
            }
            
            _context.Submissions.Remove(submission);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Hàm hỗ trợ lấy ID người dùng hiện tại
        private int GetCurrentCoderId()
        {
            // Kiểm tra từ claims
            var coderIdClaim = User.FindFirst("CoderId");
            if (coderIdClaim != null && int.TryParse(coderIdClaim.Value, out int claimCoderId))
            {
                return claimCoderId;
            }

            // Kiểm tra từ username
            if (User.Identity.IsAuthenticated)
            {
                var username = User.Identity.Name;
                var coder = _context.Coders.FirstOrDefault(c => c.CoderName == username);
                if (coder != null)
                {
                    return coder.CoderId;
                }
            }

            return -1;
        }

        // Phương thức để chuyển đổi CSV thành bảng ASCII
        private string ConvertCsvToAsciiTable(string csv)
        {
            try
            {
                // Phân tích CSV
                var lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                    return csv;

                var rows = new List<string[]>();
                foreach (var line in lines)
                {
                    rows.Add(line.Split(','));
                }

                // Tính toán độ rộng cột
                var columnWidths = new int[rows[0].Length];
                for (int col = 0; col < rows[0].Length; col++)
                {
                    columnWidths[col] = rows.Max(row => col < row.Length ? row[col].Length : 0);
                }

                // Tạo bảng ASCII
                var result = new StringBuilder();

                // Dòng đầu tiên với dấu +
                result.Append('+');
                for (int col = 0; col < columnWidths.Length; col++)
                {
                    result.Append(new string('-', columnWidths[col] + 2));
                    result.Append('+');
                }
                result.AppendLine();

                // Header
                for (int row = 0; row < rows.Count; row++)
                {
                    result.Append('|');
                    for (int col = 0; col < columnWidths.Length; col++)
                    {
                        string value = col < rows[row].Length ? rows[row][col] : "";
                        result.Append($" {value.PadRight(columnWidths[col])} |");
                    }
                    result.AppendLine();

                    // Dòng phân cách sau header hoặc cuối cùng
                    if (row == 0 || row == rows.Count - 1)
                    {
                        result.Append('+');
                        for (int col = 0; col < columnWidths.Length; col++)
                        {
                            result.Append(new string('-', columnWidths[col] + 2));
                            result.Append('+');
                        }
                        result.AppendLine();
                    }
                }

                return result.ToString();
            }
            catch
            {
                // Nếu có lỗi, trả về nguyên bản
                return csv;
            }
        }
    }
} 