using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using sql_exercise_scoring.Models;

namespace sql_exercise_scoring.Areas.Admin.Controllers
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
        public IActionResult Edit(int id, Coder coder)
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
                // Có thể thêm UpdatedBy nếu có thông tin người dùng hiện tại
                // existingCoder.UpdatedBy = currentUser.Id;

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
