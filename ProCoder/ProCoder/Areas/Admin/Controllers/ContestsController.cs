using Microsoft.AspNetCore.Mvc;
using ProCoder.Models;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class ContestsAdminController : Controller
    {
        private readonly SqlExerciseScoringContext db = new SqlExerciseScoringContext();

        [Route("")]
        [Route("Index")]
        [HttpGet]
        public IActionResult Index()
        {
            var contests = db.Contests
                             .Include(c => c.Coder)
                             .ToList();
            return View(contests);
        }

        [Route("Create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Route("Create")]
        [HttpPost]
        public IActionResult Create(Contest contest)
        {
            if (ModelState.IsValid)
            {
                // Thiết lập thời gian tạo và cập nhật
                contest.StartTime = DateTime.UtcNow;
                contest.EndTime = DateTime.UtcNow;

                // Lưu vào cơ sở dữ liệu
                db.Contests.Add(contest);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(contest);
        }

        [Route("Delete/{id:int}")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                // Tìm contest theo ID
                var contest = db.Contests.FirstOrDefault(c => c.ContestId == id);
                if (contest == null)
                {
                    return NotFound(); // Nếu không tìm thấy, trả về lỗi 404
                }

                // Xóa các thông tin liên quan (nếu có)
                var hasProblems = db.HasProblems.Where(hp => hp.ContestId == id);
                db.HasProblems.RemoveRange(hasProblems);

                var participations = db.Participations.Where(p => p.ContestId == id);
                db.Participations.RemoveRange(participations);

                var announcements = db.Announcements.Where(a => a.ContestId == id);
                db.Announcements.RemoveRange(announcements);

                // Xóa contest
                db.Contests.Remove(contest);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug
                Console.WriteLine($"Error deleting contest: {ex.ToString()}");
                TempData["ErrorMessage"] = "Không thể xóa cuộc thi này. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [Route("Detail/{id:int}")]
        [HttpGet]
        public IActionResult Detail(int id)
        {
            var contest = db.Contests.FirstOrDefault(c => c.ContestId == id);
            if (contest == null)
            {
                return NotFound();
            }
            return View(contest);
        }

        [Route("Edit/{id:int}")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var contest = db.Contests.FirstOrDefault(c => c.ContestId == id);
            if (contest == null)
            {
                return NotFound();
            }
            return View(contest);
        }

        [Route("Edit/{id:int}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Contest contest)
        {
            if (id != contest.ContestId)
            {
                return NotFound();
            }

            try
            {
                var existingContest = db.Contests.FirstOrDefault(c => c.ContestId == id);
                if (existingContest == null)
                {
                    return NotFound();
                }

                // Cập nhật các thông tin khác
                existingContest.ContestName = contest.ContestName;
                existingContest.ContestDescription = contest.ContestDescription;
                existingContest.StartTime = contest.StartTime;
                existingContest.EndTime = contest.EndTime;
                existingContest.RuleType = contest.RuleType;
                existingContest.FailedPenalty = contest.FailedPenalty;
                existingContest.Published = contest.Published;
                existingContest.StatusContest = contest.StatusContest;
                existingContest.Duration = contest.Duration;
                existingContest.RankingFinished = contest.RankingFinished;
                existingContest.FrozenTime = contest.FrozenTime;

                db.Update(existingContest);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
            }

            return View(contest);
        }
    }
}
