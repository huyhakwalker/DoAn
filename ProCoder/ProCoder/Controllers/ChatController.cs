using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;

namespace ProCoder.Controllers
{
    public class ChatController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public ChatController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            var messages = await _context.ChatMessages
                .Include(m => m.Coder)
                .OrderByDescending(m => m.SentAt) // Sắp xếp theo thời gian mới nhất
                .Take(100)
                .OrderBy(m => m.SentAt) // Sắp xếp lại theo thời gian tăng dần để hiển thị
                .Select(m => new
                {
                    User = m.Coder.CoderName,
                    Message = m.Message,
                    SentAt = m.SentAt
                })
                .ToListAsync();

            ViewBag.Messages = messages;
            return View();
        }
    }
} 