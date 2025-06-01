using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using ProCoder.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Linq;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class CodersAdminController : Controller
    {
        private SqlExerciseScoringContext db = new SqlExerciseScoringContext();

        [Route("")]
        [Route("Index")]
        [HttpGet]
        public IActionResult Index(int page = 1, int pageSize = 10, string search = null)
        {
            // Lấy danh sách coders
            var query = db.Coders.AsQueryable();

            // Tìm kiếm theo tên hoặc email, không phân biệt chữ hoa/thường
            if (!string.IsNullOrEmpty(search))
            {
                string searchLower = search.Trim().ToLower();
                query = query.Where(c => c.CoderName.ToLower().Contains(searchLower)
                                 || c.CoderEmail.ToLower().Contains(searchLower));
            }

            // Tính toán số lượng trang
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Đảm bảo trang hiện tại hợp lệ
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            // Lấy dữ liệu cho trang hiện tại
            var coders = query
                .OrderBy(c => c.CoderName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Truyền thông tin phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchString = search;
            ViewBag.TotalItems = totalItems;

            return View(coders);
        }
        [Route("Create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [Route("Create")]
        [HttpPost]
        public IActionResult Create(Coder coder)
        {
            if (ModelState.IsValid)
            {
                // Mã hóa mật khẩu với MD5
                string hashedPassword = ComputeMd5Hash(coder.Password);
                coder.Password = hashedPassword;

                // Thiết lập thời gian tạo và cập nhật
                coder.CreatedAt = DateTime.UtcNow;
                coder.UpdatedAt = DateTime.UtcNow;

                // Lưu vào cơ sở dữ liệu
                db.Coders.Add(coder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(coder);
        }

        // Hàm mã hóa MD5
        private string ComputeMd5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
        [Route("Delete/{id:int}")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                // Tìm coder theo ID
                var coder = db.Coders.FirstOrDefault(c => c.CoderId == id);
                if (coder == null)
                {
                    return NotFound(); // Nếu không tìm thấy, trả về lỗi 404
                }

                // Xóa các comments
                var comments = db.Comments.Where(c => c.CoderId == id);
                db.Comments.RemoveRange(comments);

                // Xóa các blogs
                var blogs = db.Blogs.Where(b => b.CoderId == id);
                db.Blogs.RemoveRange(blogs);

                // Xóa các participations
                var participations = db.Participations.Where(p => p.CoderId == id);
                db.Participations.RemoveRange(participations);

                // Xóa các favourites
                var favourites = db.Favourites.Where(f => f.CoderId == id);
                db.Favourites.RemoveRange(favourites);

                // Xóa các submissions và dữ liệu liên quan
                var submissions = db.Submissions.Where(s => s.CoderId == id);
                db.Submissions.RemoveRange(submissions);

                // Xóa các problems và dữ liệu liên quan
                var problems = db.Problems.Where(p => p.CoderId == id);
                foreach (var problem in problems)
                {
                    // Xóa các test cases của problem
                    var testCases = db.TestCases.Where(t => t.ProblemId == problem.ProblemId);
                    db.TestCases.RemoveRange(testCases);

                    // Xóa các has problems
                    var hasProblems = db.HasProblems.Where(hp => hp.ProblemId == problem.ProblemId);
                    db.HasProblems.RemoveRange(hasProblems);

                    // Xóa các take parts
                    var takeParts = db.TakeParts.Where(tp => tp.ProblemId == problem.ProblemId);
                    db.TakeParts.RemoveRange(takeParts);
                }
                db.Problems.RemoveRange(problems);

                // Xóa các contests
                var contests = db.Contests.Where(c => c.CoderId == id);
                db.Contests.RemoveRange(contests);

                // Cuối cùng xóa coder
                db.Coders.Remove(coder);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug
                Console.WriteLine($"Error deleting coder: {ex.ToString()}");
                TempData["ErrorMessage"] = "Không thể xóa người dùng này. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [Route("Detail/{id:int}")]
        [HttpGet]
        public IActionResult Detail(int id)
        {
            var coder = db.Coders.FirstOrDefault(c => c.CoderId == id);
            if (coder == null)
            {
                return NotFound();
            }
            return View(coder);
        }

        [Route("Edit/{id:int}")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var coder = db.Coders.FirstOrDefault(c => c.CoderId == id);
            if (coder == null)
            {
                return NotFound();
            }
            return View(coder);
        }

        [Route("Edit/{id:int}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Coder coder, IFormFile? AvatarFile)
        {
            if (id != coder.CoderId)
            {
                return NotFound();
            }

            try
            {
                var existingCoder = db.Coders.FirstOrDefault(c => c.CoderId == id);
                if (existingCoder == null)
                {
                    return NotFound();
                }

                // Xử lý upload ảnh đại diện
                if (AvatarFile != null && AvatarFile.Length > 0)
                {
                    // Kiểm tra định dạng file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(AvatarFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["ErrorMessage"] = "Chỉ chấp nhận file ảnh định dạng JPG, JPEG hoặc PNG!";
                        return View(coder);
                    }

                    // Kiểm tra kích thước file (max 2MB)
                    if (AvatarFile.Length > 2 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "Kích thước file không được vượt quá 2MB!";
                        return View(coder);
                    }

                    // Tạo tên file mới
                    string fileName = $"{Guid.NewGuid()}{fileExtension}";
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    // Tạo thư mục nếu chưa tồn tại
                    Directory.CreateDirectory(uploadsFolder);

                    // Xóa avatar cũ nếu có
                    if (!string.IsNullOrEmpty(existingCoder.CoderAvatar))
                    {
                        string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars", existingCoder.CoderAvatar);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Lưu file mới
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        AvatarFile.CopyTo(fileStream);
                    }

                    // Cập nhật đường dẫn trong database
                    existingCoder.CoderAvatar = fileName;
                }

                // Giữ nguyên mật khẩu cũ
                coder.Password = existingCoder.Password;

                // Cập nhật các thông tin khác
                existingCoder.CoderName = coder.CoderName;
                existingCoder.CoderEmail = coder.CoderEmail;
                existingCoder.Gender = coder.Gender;
                existingCoder.AdminCoder = coder.AdminCoder;
                existingCoder.ContestSetter = coder.ContestSetter;
                existingCoder.ReceiveEmail = coder.ReceiveEmail;
                existingCoder.UpdatedAt = DateTime.UtcNow;

                db.Update(existingCoder);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
            }

            return View(coder);
        }
    }
}
