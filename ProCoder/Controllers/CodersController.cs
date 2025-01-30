using Microsoft.AspNetCore.Mvc;
using ProCoder.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography;
using System.Text;

namespace ProCoder.Controllers
{
    public class CodersController : Controller
    {
        private readonly SqlExerciseScoringContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CodersController(SqlExerciseScoringContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Profile()
        {
            // Lấy thông tin user đang đăng nhập
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Home");
            }

            var coder = _context.Coders
                .FirstOrDefault(c => c.CoderEmail == userEmail);
            if (coder == null)
            {
                return NotFound();
            }

            return View(coder);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string coderName, string coderEmail, bool? gender, bool receiveEmail)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var coder = _context.Coders.FirstOrDefault(c => c.CoderEmail == userEmail);

            if (coder == null)
            {
                return NotFound();
            }

            // Kiểm tra email mới có bị trùng không
            if (coderEmail != userEmail)
            {
                var existingCoder = _context.Coders.FirstOrDefault(c => c.CoderEmail == coderEmail);
                if (existingCoder != null)
                {
                    TempData["ErrorMessage"] = "Email này đã được sử dụng!";
                    return RedirectToAction(nameof(Profile));
                }
            }

            // Cập nhật thông tin
            coder.CoderName = coderName;
            coder.CoderEmail = coderEmail;
            coder.Gender = gender;
            coder.ReceiveEmail = receiveEmail;
            coder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var coder = _context.Coders.FirstOrDefault(c => c.CoderEmail == userEmail);

            if (coder == null)
            {
                return NotFound();
            }

            // Kiểm tra mật khẩu hiện tại
            if (HashPassword(currentPassword) != coder.Password)
            {
                TempData["ErrorMessage"] = "Mật khẩu hiện tại không đúng!";
                return RedirectToAction(nameof(Profile));
            }

            // Kiểm tra mật khẩu mới
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp!";
                return RedirectToAction(nameof(Profile));
            }

            if (newPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới phải có ít nhất 6 ký tự!";
                return RedirectToAction(nameof(Profile));
            }

            // Cập nhật mật khẩu
            coder.Password = HashPassword(newPassword);
            coder.UpdatedAt = DateTime.UtcNow;
            coder.UpdatedBy = userEmail;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeAvatar(IFormFile avatarFile)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var coder = _context.Coders.FirstOrDefault(c => c.CoderEmail == userEmail);

            if (coder == null)
            {
                return NotFound();
            }

            if (avatarFile != null && avatarFile.Length > 0)
            {
                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(avatarFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessage"] = "Chỉ chấp nhận file ảnh định dạng JPG, JPEG hoặc PNG!";
                    return RedirectToAction(nameof(Profile));
                }

                // Kiểm tra kích thước file (max 2MB)
                if (avatarFile.Length > 2 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Kích thước file không được vượt quá 2MB!";
                    return RedirectToAction(nameof(Profile));
                }

                try
                {
                    // Tạo tên file mới
                    string fileName = $"{Guid.NewGuid()}{fileExtension}";
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    // Tạo thư mục nếu chưa tồn tại
                    Directory.CreateDirectory(uploadsFolder);

                    // Xóa avatar cũ nếu có
                    if (!string.IsNullOrEmpty(coder.CoderAvatar))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars", coder.CoderAvatar);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Lưu file mới
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(fileStream);
                    }

                    // Cập nhật đường dẫn trong database
                    coder.CoderAvatar = fileName;
                    coder.UpdatedAt = DateTime.UtcNow;
                    coder.UpdatedBy = userEmail;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật ảnh đại diện thành công!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật ảnh đại diện!";
                }
            }

            return RedirectToAction(nameof(Profile));
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
