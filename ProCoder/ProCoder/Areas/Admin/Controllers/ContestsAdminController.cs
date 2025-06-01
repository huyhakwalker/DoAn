using Microsoft.AspNetCore.Mvc;
using ProCoder.Models;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize] // Yêu cầu đăng nhập
    public class ContestsAdminController : Controller
    {
        private readonly SqlExerciseScoringContext _context;
        private readonly ILogger<ContestsAdminController> _logger;

        public ContestsAdminController(SqlExerciseScoringContext context, ILogger<ContestsAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Helper method để lấy thông tin người dùng đang đăng nhập
        private async Task<Coder> GetCurrentCoderAsync()
        {
            var coderName = User.Identity?.Name;
            if (!string.IsNullOrEmpty(coderName))
            {
                var coder = await _context.Coders
                    .FirstOrDefaultAsync(c => c.CoderName == coderName);
                return coder;
            }
            return null;
        }

        [Route("")]
        [Route("Index")]
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = null)
        {
            try
            {
                var query = _context.Contests
                    .Include(c => c.Coder)
                    .AsQueryable();
                    
                // Tìm kiếm theo tên cuộc thi hoặc người tạo
                if (!string.IsNullOrEmpty(search))
                {
                    string searchLower = search.Trim().ToLower();
                    query = query.Where(c => c.ContestName.ToLower().Contains(searchLower)
                                     || (c.Coder != null && c.Coder.CoderName.ToLower().Contains(searchLower)));
                }
                
                // Tính toán số lượng trang
                int totalItems = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                
                // Đảm bảo trang hiện tại hợp lệ
                page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

                // Lấy dữ liệu cho trang hiện tại
                var contests = await query
                    .OrderByDescending(c => c.ContestId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                    
                // Truyền thông tin phân trang
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.SearchString = search;
                ViewBag.TotalItems = totalItems;
                
                return View(contests);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading contests: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return View(new List<Contest>());
            }
        }

        [Route("Create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Lấy thông tin người dùng hiện tại
                var currentCoder = await GetCurrentCoderAsync();

                if (currentCoder == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("Index");
                }

                // Tạo contest mới với CoderId được set sẵn
                var contest = new Contest
                {
                    CoderId = currentCoder.CoderId,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddHours(2),
                    Duration = 120,
                    StatusContest = "Pending",
                    Published = false
                };

                ViewBag.CurrentCoder = currentCoder;
                ViewBag.Coders = await _context.Coders.ToListAsync(); // Vẫn giữ lại để admin có thể chọn người khác nếu cần
                return View(contest);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Create GET: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContestName,ContestDescription,StartTime,EndTime,Duration,CoderId,StatusContest,Published")] Contest contest)
        {
            try
            {
                // Debug: Ghi log giá trị Published được gửi lên
                _logger.LogInformation($"Create: Published value from form: {contest.Published}");
                
                // Nếu không có CoderId được chọn, sử dụng CoderId của người đang đăng nhập
                if (contest.CoderId == 0)
                {
                    var currentCoder = await GetCurrentCoderAsync();
                    if (currentCoder != null)
                    {
                        contest.CoderId = currentCoder.CoderId;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                        ViewBag.Coders = await _context.Coders.ToListAsync();
                        return View(contest);
                    }
                }

                // Thêm đối tượng mới vào DbSet
                _context.Contests.Add(contest);
                
                // Debug: Ghi log giá trị Published trước khi lưu
                _logger.LogInformation($"Create: Published value before SaveChanges: {contest.Published}");
                
                await _context.SaveChangesAsync();
                
                // Debug: Truy vấn lại để kiểm tra giá trị đã lưu
                var savedContest = await _context.Contests.FindAsync(contest.ContestId);
                _logger.LogInformation($"Create: Published value after SaveChanges: {savedContest?.Published}");
                
                TempData["SuccessMessage"] = "Tạo mới cuộc thi thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Ghi log chi tiết lỗi
                _logger.LogError($"Error in Create POST: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = $"Lỗi khi tạo cuộc thi: {ex.Message}";
            }
            
            // Trong trường hợp lỗi, load lại danh sách Coders
            ViewBag.Coders = await _context.Coders.ToListAsync();
            ViewBag.CurrentCoder = await GetCurrentCoderAsync();
            return View(contest);
        }

        [Route("Delete/{id:int}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Tìm contest theo ID
                var contest = await _context.Contests.FirstOrDefaultAsync(c => c.ContestId == id);
                if (contest == null)
                {
                    return NotFound(); // Nếu không tìm thấy, trả về lỗi 404
                }

                // Xóa các thông tin liên quan (nếu có)
                var hasProblems = await _context.HasProblems.Where(hp => hp.ContestId == id).ToListAsync();
                _context.HasProblems.RemoveRange(hasProblems);

                var participations = await _context.Participations.Where(p => p.ContestId == id).ToListAsync();
                _context.Participations.RemoveRange(participations);

                // Xóa contest
                _context.Contests.Remove(contest);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa cuộc thi thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug
                _logger.LogError($"Error deleting contest: {ex.Message}");
                TempData["ErrorMessage"] = "Không thể xóa cuộc thi này. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [Route("Detail/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var contest = await _context.Contests
                .Include(c => c.Coder)
                .Include(c => c.HasProblems)
                    .ThenInclude(hp => hp.Problem)
                        .ThenInclude(p => p.Theme)
                .FirstOrDefaultAsync(c => c.ContestId == id);
                
            if (contest == null)
            {
                return NotFound();
            }
            
            // Debug: Ghi log giá trị Published khi hiển thị chi tiết
            _logger.LogInformation($"Detail: Contest ID {id}, Published value: {contest.Published}");
            
            // Lấy danh sách các bài tập đang không thuộc contest này để hiển thị trong dropdown
            var contestProblemIds = contest.HasProblems.Select(hp => hp.ProblemId).ToList();
            ViewBag.AvailableProblems = await _context.Problems
                .Where(p => !contestProblemIds.Contains(p.ProblemId))
                .Select(p => new SelectListItem
                {
                    Value = p.ProblemId.ToString(),
                    Text = $"{p.ProblemCode} - {p.ProblemName}"
                })
                .ToListAsync();
                
            return View(contest);
        }

        [Route("Edit/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var contest = await _context.Contests.FirstOrDefaultAsync(c => c.ContestId == id);
            if (contest == null)
            {
                return NotFound();
            }
            ViewBag.Coders = await _context.Coders.ToListAsync();
            ViewBag.CurrentCoder = await GetCurrentCoderAsync();
            return View(contest);
        }

        [Route("Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContestId,ContestName,ContestDescription,StartTime,EndTime,Duration,CoderId,StatusContest,Published")] Contest contest)
        {
            if (id != contest.ContestId)
            {
                return NotFound();
            }

            try
            {
                // Debug: Ghi log giá trị Published được gửi lên
                _logger.LogInformation($"Edit: Published value from form: {contest.Published}");

                // Tìm contest hiện tại từ database
                var existingContest = await _context.Contests.FindAsync(id);
                if (existingContest == null)
                {
                    return NotFound();
                }

                _logger.LogInformation($"Edit: Current Published value in DB before update: {existingContest.Published}");

                // Cập nhật các thuộc tính
                existingContest.ContestName = contest.ContestName;
                existingContest.ContestDescription = contest.ContestDescription;
                existingContest.StartTime = contest.StartTime;
                existingContest.EndTime = contest.EndTime;
                existingContest.Duration = contest.Duration;
                existingContest.CoderId = contest.CoderId;
                existingContest.StatusContest = contest.StatusContest;
                existingContest.Published = contest.Published;

                _logger.LogInformation($"Edit: Published value after update: {existingContest.Published}");

                // Cập nhật vào database
                await _context.SaveChangesAsync();
                
                // Kiểm tra lại giá trị sau khi lưu
                var updatedContest = await _context.Contests.FindAsync(id);
                _logger.LogInformation($"Edit: Published value after SaveChanges: {updatedContest.Published}");
                
                TempData["SuccessMessage"] = "Cập nhật cuộc thi thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Ghi log chi tiết lỗi
                _logger.LogError($"Error in Edit POST: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật cuộc thi: {ex.Message}";
            }
            
            ViewBag.Coders = await _context.Coders.ToListAsync();
            ViewBag.CurrentCoder = await GetCurrentCoderAsync();
            return View(contest);
        }

        [Route("AddProblem")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProblem(int contestId, int problemId, int problemOrder, int pointProblem)
        {
            try
            {
                _logger.LogInformation($"AddProblem called with contestId={contestId}, problemId={problemId}, problemOrder={problemOrder}, pointProblem={pointProblem}");
                
                // Kiểm tra contest và problem tồn tại
                var contest = await _context.Contests.FindAsync(contestId);
                if (contest == null)
                {
                    _logger.LogError($"Contest with ID {contestId} not found");
                    TempData["ErrorMessage"] = $"Không tìm thấy cuộc thi với ID {contestId}";
                    return RedirectToAction("Index");
                }
                
                var problem = await _context.Problems.FindAsync(problemId);
                if (problem == null)
                {
                    _logger.LogError($"Problem with ID {problemId} not found");
                    TempData["ErrorMessage"] = $"Không tìm thấy bài tập với ID {problemId}";
                    return RedirectToAction("Detail", new { id = contestId });
                }
                
                // Kiểm tra xem bài tập đã có trong contest chưa
                var existing = await _context.HasProblems
                    .FirstOrDefaultAsync(hp => hp.ContestId == contestId && hp.ProblemId == problemId);
                    
                if (existing != null)
                {
                    _logger.LogWarning($"Problem {problemId} already exists in contest {contestId}");
                    TempData["ErrorMessage"] = "Bài tập này đã có trong cuộc thi";
                    return RedirectToAction("Detail", new { id = contestId });
                }
                
                // Tạo mới HasProblem
                var hasProblem = new HasProblem
                {
                    ContestId = contestId,
                    ProblemId = problemId,
                    ProblemOrder = problemOrder,
                    PointProblem = pointProblem
                };
                
                // Thêm vào context và lưu
                _logger.LogInformation($"Adding HasProblem: ContestId={hasProblem.ContestId}, ProblemId={hasProblem.ProblemId}");
                _context.HasProblems.Add(hasProblem);
                await _context.SaveChangesAsync();
                _logger.LogInformation("HasProblem added successfully");
                
                TempData["SuccessMessage"] = "Thêm bài tập vào cuộc thi thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding problem to contest: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Lỗi khi thêm bài tập: {ex.Message}";
            }
            
            return RedirectToAction("Detail", new { id = contestId });
        }
        
        [Route("RemoveProblem")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProblem(int contestId, int problemId)
        {
            try
            {
                var hasProblem = await _context.HasProblems
                    .FirstOrDefaultAsync(hp => hp.ContestId == contestId && hp.ProblemId == problemId);
                    
                if (hasProblem == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bài tập trong cuộc thi";
                    return RedirectToAction("Detail", new { id = contestId });
                }
                
                _context.HasProblems.Remove(hasProblem);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Xóa bài tập khỏi cuộc thi thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing problem from contest: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi xóa bài tập: {ex.Message}";
            }
            
            return RedirectToAction("Detail", new { id = contestId });
        }
    }
}
