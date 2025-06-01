using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using Microsoft.AspNetCore.Authorization;

namespace ProCoder.Controllers
{
    [Authorize]
    public class ParticipationsController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public ParticipationsController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        // POST: Participations/Register/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Không tìm thấy cuộc thi" });
            }

            var contest = await _context.Contests.FindAsync(id);
            
            if (contest == null)
            {
                return Json(new { success = false, message = "Không tìm thấy cuộc thi" });
            }

            // Check if the contest is open for registration (only Pending or Running)
            if (contest.StatusContest != "Pending" && contest.StatusContest != "Running")
            {
                return Json(new { success = false, message = "Cuộc thi đã kết thúc, không thể đăng ký." });
            }

            // Tìm CoderId bằng nhiều cách
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
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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
                return Json(new { success = false, message = "Không thể xác định người dùng" });
            }

            // Check if already registered
            var existingParticipation = await _context.Participations
                .FirstOrDefaultAsync(p => p.CoderId == coderId.Value && p.ContestId == id);
                
            if (existingParticipation != null)
            {
                return Json(new { success = true, isRegistered = true, message = "Bạn đã đăng ký tham gia cuộc thi này." });
            }

            // Create new participation
            var participation = new Participation
            {
                CoderId = coderId.Value,
                ContestId = contest.ContestId,
                RegisterTime = DateTime.Now,
                PointScore = 0,
                TimeScore = 0,
                Ranking = 0,
                SolvedCount = 0
            };

            _context.Add(participation);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, isRegistered = true, message = "Đăng ký tham gia cuộc thi thành công!" });
        }
    }
} 