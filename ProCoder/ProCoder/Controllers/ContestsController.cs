using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;

namespace ProCoder.Controllers
{
    public class ContestsController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public ContestsController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        // GET: Contests
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            var currentTime = DateTime.UtcNow;

            // Lấy danh sách tất cả các cuộc thi đã công khai (Published)
            var contests = await _context.Contests
                .Where(c => c.Published)
                .OrderBy(c => c.StartTime)
                .Include(c => c.Coder) // Bao gồm thông tin người tạo cuộc thi
                .ToListAsync();
            
            // Tính số lượng người tham gia cho từng cuộc thi
            var contestIds = contests.Select(c => c.ContestId).ToList();
            var participantCounts = await _context.Participations
                .Where(p => contestIds.Contains(p.ContestId))
                .GroupBy(p => p.ContestId)
                .Select(g => new { ContestId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ContestId, x => x.Count);
            
            ViewBag.ContestParticipantCounts = participantCounts;

            // Tìm CoderId của người dùng hiện tại
            int? coderId = null;
            
            // Cách 1: Tìm từ claim trực tiếp
            var coderIdClaim = User.FindFirst("CoderId")?.Value;
            if (!string.IsNullOrEmpty(coderIdClaim) && int.TryParse(coderIdClaim, out int cId))
            {
                coderId = cId;
            }
            
            // Cách 2: Tìm từ UserId
            if (!coderId.HasValue)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CreatedBy == userId);
                    if (coder != null)
                    {
                        coderId = coder.CoderId;
                    }
                }
            }
            
            // Cách 3: Tìm từ Username
            if (!coderId.HasValue)
            {
                var username = User.Identity.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CoderName == username);
                    if (coder != null)
                    {
                        coderId = coder.CoderId;
                    }
                }
            }
            
            // Lấy danh sách các cuộc thi mà người dùng đã đăng ký
            if (coderId.HasValue)
            {
                var registeredContestIds = await _context.Participations
                    .Where(p => p.CoderId == coderId.Value)
                    .Select(p => p.ContestId)
                    .ToListAsync();
                
                ViewBag.RegisteredContests = registeredContestIds;
            }
            else
            {
                ViewBag.RegisteredContests = new List<int>();
            }

            // Cập nhật trạng thái cuộc thi
            foreach (var contest in contests)
            {
                // Không thay đổi trạng thái của cuộc thi đã được đánh dấu là kết thúc
                if (contest.StatusContest == "Finished")
                {
                    continue; // Bỏ qua cuộc thi đã kết thúc
                }
                
                // Kiểm tra nếu trạng thái đã được admin thiết lập thủ công
                bool isManuallySet = false;
                
                // Logic để phát hiện trạng thái đã được thiết lập thủ công
                // Nếu trạng thái là "Running" nhưng thời gian hiện tại không nằm giữa StartTime và EndTime
                if (contest.StatusContest == "Running" && 
                    (currentTime < contest.StartTime || currentTime > contest.EndTime))
                {
                    isManuallySet = true; // Trạng thái đã được admin thiết lập thủ công
                }
                
                // Hoặc nếu trạng thái là "Pending" nhưng thời gian hiện tại đã sau StartTime
                if (contest.StatusContest == "Pending" && currentTime >= contest.StartTime)
                {
                    isManuallySet = true; // Trạng thái đã được admin thiết lập thủ công
                }
                
                // Bỏ qua việc cập nhật nếu trạng thái đã được thiết lập thủ công
                if (isManuallySet)
                {
                    continue;
                }
                
                // Cập nhật trạng thái dựa trên thời gian nếu không được thiết lập thủ công
                if (currentTime > contest.EndTime && contest.StatusContest != "Finished")
                {
                    contest.StatusContest = "Finished";
                    _context.Contests.Update(contest);
                }
                else if (currentTime >= contest.StartTime && currentTime <= contest.EndTime && contest.StatusContest != "Running")
                {
                    contest.StatusContest = "Running";
                    _context.Contests.Update(contest);
                }
                else if (currentTime < contest.StartTime && contest.StatusContest != "Pending")
                {
                    contest.StatusContest = "Pending";
                    _context.Contests.Update(contest);
                }
            }
            
            await _context.SaveChangesAsync();

            return View(contests); // Trả về danh sách cuộc thi
        }

        // GET: Contests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var contest = await _context.Contests
                .Include(c => c.Coder)
                .Include(c => c.HasProblems)
                    .ThenInclude(hp => hp.Problem)
                        .ThenInclude(p => p.Theme)
                .FirstOrDefaultAsync(m => m.ContestId == id);

            if (contest == null)
            {
                return NotFound();
            }

            // Check if current time is outside the contest period and update status if needed
            var currentTime = DateTime.UtcNow;
            
            // Không thay đổi trạng thái đã được đánh dấu Finished
            if (contest.StatusContest == "Finished")
            {
                // Không làm gì, giữ nguyên trạng thái
            }
            else
            {
                // Kiểm tra nếu trạng thái đã được admin thiết lập thủ công
                bool isManuallySet = false;
                
                // Logic để phát hiện trạng thái đã được thiết lập thủ công
                // Nếu trạng thái là "Running" nhưng thời gian hiện tại không nằm giữa StartTime và EndTime
                if (contest.StatusContest == "Running" && 
                    (currentTime < contest.StartTime || currentTime > contest.EndTime))
                {
                    isManuallySet = true; // Trạng thái đã được admin thiết lập thủ công
                }
                
                // Hoặc nếu trạng thái là "Pending" nhưng thời gian hiện tại đã sau StartTime
                if (contest.StatusContest == "Pending" && currentTime >= contest.StartTime)
                {
                    isManuallySet = true; // Trạng thái đã được admin thiết lập thủ công
                }
                
                // Cập nhật trạng thái dựa trên thời gian nếu không được thiết lập thủ công
                if (!isManuallySet)
                {
                    if (currentTime > contest.EndTime && contest.StatusContest != "Finished")
                    {
                        contest.StatusContest = "Finished";
                        await _context.SaveChangesAsync();
                    }
                    else if (currentTime >= contest.StartTime && currentTime <= contest.EndTime && contest.StatusContest != "Running")
                    {
                        contest.StatusContest = "Running";
                        await _context.SaveChangesAsync();
                    }
                    else if (currentTime < contest.StartTime && contest.StatusContest != "Pending")
                    {
                        contest.StatusContest = "Pending";
                        await _context.SaveChangesAsync();
                    }
                }
            }

            if (User.Identity.IsAuthenticated)
            {
                // Lấy Coder ID
                int coderId = 0;
                var username = User.Identity.Name;
                var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CoderName == username);
                
                if (coder != null)
                {
                    coderId = coder.CoderId;
                    
                    // Kiểm tra xem người dùng đã đăng ký cho cuộc thi này chưa
                    var isRegistered = await _context.Participations
                        .AnyAsync(p => p.CoderId == coderId && p.ContestId == id);
                    
                    ViewBag.IsUserRegistered = isRegistered;
                    
                    // Nếu đã đăng ký, lấy thông tin về TakePart
                    if (isRegistered)
                    {
                        var participation = await _context.Participations
                            .FirstOrDefaultAsync(p => p.CoderId == coderId && p.ContestId == id);
                            
                        if (participation != null)
                        {
                            // Lấy TakePart cho mỗi bài tập trong cuộc thi
                            var takeParts = await _context.TakeParts
                                .Where(tp => tp.ParticipationId == participation.ParticipationId)
                                .ToListAsync();
                                
                            ViewBag.TakeParts = takeParts.ToDictionary(tp => tp.ProblemId, tp => tp);
                        }
                    }
                }
                else
                {
                    ViewBag.IsUserRegistered = false;
                }
            }
            else
            {
                ViewBag.IsUserRegistered = false;
            }

            return View(contest);
        }

        // GET: Contests/Leaderboard/5
        public async Task<IActionResult> Leaderboard(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contest = await _context.Contests
                .Include(c => c.Coder)
                .FirstOrDefaultAsync(m => m.ContestId == id);

            if (contest == null)
            {
                return NotFound();
            }

            // Lấy tất cả bài tham gia của cuộc thi
            var contestProblems = await _context.HasProblems
                .Where(hp => hp.ContestId == id)
                .Include(hp => hp.Problem)
                .ToListAsync();

            ViewBag.ContestProblems = contestProblems;

            // Lấy tất cả người tham gia cuộc thi
            var participations = await _context.Participations
                .Where(p => p.ContestId == id)
                .Include(p => p.Coder)
                .ToListAsync();

            var leaderboardData = new List<dynamic>();

            foreach (var participation in participations)
            {
                // Lấy tất cả bài làm của người dùng trong cuộc thi
                var takePartEntries = await _context.TakeParts
                    .Where(tp => tp.ParticipationId == participation.ParticipationId)
                    .ToListAsync();

                // Tổng điểm
                double totalPoints = takePartEntries.Sum(tp => tp.PointWon != null ? tp.PointWon : 0);
                
                // Tổng thời gian - Chỉ tính những bài đã giải được
                int totalTime = 0;
                foreach (var takePart in takePartEntries.Where(tp => tp.TimeSolved.HasValue))
                {
                    // Tính thời gian làm bài (tính bằng phút từ lúc bắt đầu cuộc thi)
                    DateTime startTime = contest.StartTime;
                    DateTime solvedTime = takePart.TimeSolved.Value;
                    
                    // Đảm bảo thời gian giải không sớm hơn thời gian bắt đầu
                    if (solvedTime < startTime)
                    {
                        solvedTime = startTime;
                    }
                    
                    // Đảm bảo thời gian giải không muộn hơn thời gian kết thúc
                    if (solvedTime > contest.EndTime)
                    {
                        solvedTime = contest.EndTime;
                    }
                    
                    int minutesTaken = (int)(solvedTime - startTime).TotalMinutes;
                    
                    // Đảm bảo thời gian không âm và không vượt quá thời gian của cuộc thi
                    minutesTaken = Math.Max(0, Math.Min(minutesTaken, contest.Duration));
                    
                    totalTime += minutesTaken;
                }

                // Số lượng bài đã giải
                int problemsSolved = takePartEntries.Count(tp => tp.TimeSolved.HasValue);

                // Chi tiết từng bài
                var problemDetails = new List<dynamic>();
                foreach (var problem in contestProblems)
                {
                    var takePart = takePartEntries.FirstOrDefault(tp => 
                        _context.Submissions
                            .Any(s => s.TakePartId == tp.TakePartId && 
                                 s.ProblemId == problem.ProblemId));

                    problemDetails.Add(new
                    {
                        ProblemId = problem.ProblemId,
                        PointWon = takePart?.PointWon != null ? takePart.PointWon : 0,
                        TimeSolved = takePart?.TimeSolved,
                        Attempts = takePart?.SubmissionCount != null ? takePart.SubmissionCount : 0,
                        Solved = takePart?.TimeSolved.HasValue ?? false
                    });
                }

                leaderboardData.Add(new
                {
                    ParticipationId = participation.ParticipationId,
                    CoderId = participation.CoderId,
                    CoderName = participation.Coder.CoderName,
                    TotalPoints = totalPoints,
                    TotalTime = totalTime,
                    ProblemsSolved = problemsSolved,
                    ProblemDetails = problemDetails
                });
            }

            // Convert to list instead of using dynamic operators for sorting
            var sortedData = leaderboardData
                .Select(d => new 
                {
                    Data = d,
                    TotalPoints = (double)d.TotalPoints,
                    TotalTime = (int)d.TotalTime
                })
                .OrderByDescending(d => d.TotalPoints)
                .ThenBy(d => d.TotalTime)
                .Select(d => d.Data)
                .ToList();

            ViewBag.LeaderboardData = sortedData;
            ViewBag.Contest = contest;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(int contestId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để đăng ký cuộc thi." });
            }

            // Tìm cuộc thi
            var contest = await _context.Contests.FindAsync(contestId);
            if (contest == null)
            {
                return Json(new { success = false, message = "Không tìm thấy cuộc thi." });
            }

            // Nếu cuộc thi đã kết thúc
            if (contest.EndTime < DateTime.UtcNow)
            {
                return Json(new { success = false, message = "Cuộc thi đã kết thúc, không thể đăng ký." });
            }

            // Tìm CoderId của người dùng
            int? coderId = null;
            
            // Cách 1: Tìm từ claim trực tiếp
            var coderIdClaim = User.FindFirst("CoderId")?.Value;
            if (!string.IsNullOrEmpty(coderIdClaim) && int.TryParse(coderIdClaim, out int cId))
            {
                coderId = cId;
            }
            
            // Cách 2: Tìm từ UserId
            if (!coderId.HasValue)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CreatedBy == userId);
                    if (coder != null)
                    {
                        coderId = coder.CoderId;
                    }
                }
            }
            
            // Cách 3: Tìm từ Username
            if (!coderId.HasValue)
            {
                var username = User.Identity.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CoderName == username);
                    if (coder != null)
                    {
                        coderId = coder.CoderId;
                    }
                }
            }

            if (!coderId.HasValue)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
            }

            // Kiểm tra xem đã đăng ký chưa
            var existingParticipation = await _context.Participations
                .FirstOrDefaultAsync(p => p.ContestId == contestId && p.CoderId == coderId.Value);

            if (existingParticipation != null)
            {
                return Json(new { success = true, message = "Bạn đã đăng ký cuộc thi này rồi." });
            }

            // Tạo đăng ký mới
            var participation = new ProCoder.Models.Participation
            {
                ContestId = contestId,
                CoderId = coderId.Value,
                RegisterTime = DateTime.UtcNow
            };

            _context.Participations.Add(participation);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đăng ký cuộc thi thành công!" });
        }
    }
}