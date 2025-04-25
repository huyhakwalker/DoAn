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
        public IActionResult Index()
        {
            var coders = db.Coders.ToList();
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
            // Tìm coder theo ID
            var coder = db.Coders.FirstOrDefault(c => c.CoderId == id);
            if (coder == null)
            {
                return NotFound(); // Nếu không tìm thấy, trả về lỗi 404
            }

            try
            {
                // Xóa coder khỏi cơ sở dữ liệu
                db.Coders.Remove(coder);
                db.SaveChanges();

                // Quay lại trang danh sách sau khi xóa thành công
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                TempData["ErrorMessage"] = $"Error deleting coder: {ex.Message}";
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
