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
                // Tạo salt ngẫu nhiên
                string salt = GenerateSalt();

                // Mã hóa mật khẩu với MD5 kèm salt
                string hashedPassword = ComputeMd5Hash(coder.PwdMd5 + salt);

                // Gán giá trị vào model
                coder.PwdMd5 = hashedPassword;
                coder.SaltMd5 = salt;

                // Lưu vào cơ sở dữ liệu
                db.Coders.Add(coder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(coder);
        }

        // Hàm tạo salt ngẫu nhiên
        private string GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] saltBytes = new byte[16];
                rng.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }
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
        [Route("DetailOrUpdate/{id:int}")]
        [HttpGet]
        public IActionResult DetailOrUpdate(int id)
        {
            var coder = db.Coders.FirstOrDefault(c => c.CoderId == id);
            if (coder == null)
            {
                return NotFound(); // Nếu không tìm thấy, trả về 404
            }

            return View(coder); // Trả về view với dữ liệu coder
        }

        [Route("DetailOrUpdate/{id:int}")]
        [HttpPost]
        public IActionResult DetailOrUpdate(Coder coder)
        {
            if (ModelState.IsValid)
            {
                var existingCoder = db.Coders.FirstOrDefault(c => c.CoderId == coder.CoderId);
                if (existingCoder == null)
                {
                    return NotFound();
                }
                // Cập nhật thông tin từ form vào thực thể hiện tại
                existingCoder.CoderName = coder.CoderName;
                existingCoder.CoderEmail = coder.CoderEmail;
                existingCoder.Gender = coder.Gender;
                existingCoder.AdminCoder = coder.AdminCoder;
                existingCoder.ContestSetter = coder.ContestSetter;
                existingCoder.DescriptionCoder = coder.DescriptionCoder;

                db.Coders.Update(existingCoder); // Đánh dấu thực thể đã thay đổi
                db.SaveChanges(); // Lưu thay đổi
                return RedirectToAction("DetailOrUpdate", new { id = coder.CoderId });
            }

            // Nếu ModelState không hợp lệ, trả về lại view với dữ liệu hiện tại
            return View(coder);
        }
    }
}
