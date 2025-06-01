using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace ProCoder.Controllers
{
    [Authorize]
    public class SolvedController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public SolvedController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        // GET: Solved - Hiển thị danh sách các bài tập đã giải của người dùng hiện tại
        public async Task<IActionResult> Index()
        {
            var coderId = GetCurrentCoderId();
            if (coderId <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            var solvedProblems = await _context.Problems
                .Include(p => p.Theme)
                .Where(p => p.Coders.Any(c => c.CoderId == coderId))
                .OrderBy(p => p.ProblemId)
                .ToListAsync();

            // Lấy danh sách tất cả các themes
            var themes = await _context.Themes.OrderBy(c => c.ThemeName).ToListAsync();
            ViewBag.Themes = themes;

            // Tổng số bài đã giải
            ViewBag.ProblemCount = solvedProblems.Count();

            // Lấy thông tin người dùng hiện tại
            var currentUser = await _context.Coders.FirstOrDefaultAsync(c => c.CoderId == coderId);
            ViewBag.CurrentUser = currentUser;

            return View(solvedProblems);
        }

        // GET: Solved/Debug - Để debug các bản ghi trong bảng Solved
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Debug()
        {
            // Lấy tất cả bản ghi từ bảng Solved
            var solvedRecords = await _context.Coders
                .Include(c => c.ProblemsNavigation)
                .SelectMany(c => c.ProblemsNavigation.Select(p => new 
                {
                    CoderId = c.CoderId,
                    CoderName = c.CoderName,
                    ProblemId = p.ProblemId,
                    ProblemName = p.ProblemName,
                    ProblemCode = p.ProblemCode
                }))
                .ToListAsync();

            // Hiển thị tổng số bản ghi
            ViewBag.TotalRecords = solvedRecords.Count;
            
            // Hiển thị tất cả các bản ghi theo từng người dùng
            var groupedRecords = solvedRecords
                .GroupBy(r => new { r.CoderId, r.CoderName })
                .Select(g => new 
                {
                    Coder = g.Key,
                    Problems = g.Select(p => new 
                    {
                        p.ProblemId,
                        p.ProblemName,
                        p.ProblemCode
                    }).ToList()
                })
                .ToList();
            
            ViewBag.GroupedRecords = groupedRecords;

            return View();
        }

        // GET: Solved/GetSolvedProblems - Lấy danh sách ID bài tập đã giải
        [HttpGet]
        public async Task<IActionResult> GetSolvedProblems()
        {
            var coderId = GetCurrentCoderId();
            if (coderId <= 0)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
            }

            var solvedProblemIds = await _context.Coders
                .Where(c => c.CoderId == coderId)
                .SelectMany(c => c.ProblemsNavigation.Select(p => p.ProblemId))
                .ToListAsync();

            return Json(new { success = true, solvedProblems = solvedProblemIds });
        }

        // GET: Solved/Count - Lấy số lượng bài đã giải của người dùng hiện tại
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var coderId = GetCurrentCoderId();
            if (coderId <= 0)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
            }

            var solvedCount = await _context.Coders
                .Where(c => c.CoderId == coderId)
                .Select(c => c.ProblemsNavigation.Count())
                .FirstOrDefaultAsync();

            return Json(new { success = true, count = solvedCount });
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
    }
} 