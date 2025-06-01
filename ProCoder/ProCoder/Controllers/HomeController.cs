using Microsoft.AspNetCore.Mvc;
using ProCoder.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;
using ProCoder.Services;
using System.Web;

namespace ProCoder.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SqlExerciseScoringContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordResetService _passwordResetService;
        private readonly IEmailService _emailService;
        private static readonly Dictionary<string, (int attempts, DateTime lastAttempt)> _resetAttempts = new();
        private const int MaxResetAttemptsPerHour = 3;

        public HomeController(
            ILogger<HomeController> logger, 
            SqlExerciseScoringContext context, 
            IConfiguration configuration,
            IPasswordResetService passwordResetService,
            IEmailService emailService)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _passwordResetService = passwordResetService;
            _emailService = emailService;

            // Tạo tài khoản mặc định nếu chưa tồn tại
            CreateDefaultAccounts();
        }

        private string GetMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private void CreateDefaultAccounts()
        {
            var defaultPassword = GetMD5Hash("train123");

            // Xử lý tài khoản user
            var existingUser = _context.Coders.FirstOrDefault(c => c.CoderName == "codertest");
            if (existingUser == null)
            {
                var user = new Coder
                {
                    CoderName = "codertest",
                    CoderEmail = "codertest@example.com",
                    Password = defaultPassword,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    AdminCoder = false,
                    ContestSetter = false,
                    ReceiveEmail = true
                };
                _context.Coders.Add(user);
            }
            else
            {
                existingUser.Password = defaultPassword;
                existingUser.UpdatedAt = DateTime.UtcNow;
            }

            // Xử lý tài khoản admin
            var existingAdmin = _context.Coders.FirstOrDefault(c => c.CoderName == "admintest");
            if (existingAdmin == null)
            {
                var admin = new Coder
                {
                    CoderName = "admintest",
                    CoderEmail = "admintest@example.com",
                    Password = defaultPassword,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    AdminCoder = true,
                    ContestSetter = true,
                    ReceiveEmail = true
                };
                _context.Coders.Add(admin);
            }
            else
            {
                existingAdmin.Password = defaultPassword;
                existingAdmin.UpdatedAt = DateTime.UtcNow;
            }

            // Lưu thay đổi vào database
            _context.SaveChanges();
        }

        public IActionResult Index()
        {
            var recentProblems = _context.Problems
                .Include(p => p.DatabaseSchema)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToList();

            var viewModel = new HomeViewModel
            {
                PinnedBlogs = _context.Blogs
                    .Where(b => b.PinHome && b.Published)
                    .OrderByDescending(b => b.BlogDate)
                    .Take(5)
                    .Include(b => b.Coder)
                    .ToList(),

                TopCoders = _context.Coders
                    .OrderByDescending(c => c.Submissions
                        .Where(s => s.SubmissionStatus == "Accepted")
                        .Select(s => s.ProblemId)
                        .Distinct()
                        .Count())
                    .Take(5)
                    .Select(c => new TopCoderViewModel 
                    { 
                        CoderName = c.CoderName,
                        SubmissionCount = c.Submissions
                            .Where(s => s.SubmissionStatus == "Accepted")
                            .Select(s => s.ProblemId)
                            .Distinct()
                            .Count()
                    })
                    .ToList(),

                RecentProblems = recentProblems
            };

            return View(viewModel);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string CoderEmail, string Password)
        {
            if (!string.IsNullOrEmpty(CoderEmail) && !string.IsNullOrEmpty(Password))
            {
                var hashedPassword = GetMD5Hash(Password);
                var coder = await _context.Coders
                    .FirstOrDefaultAsync(c => c.CoderEmail == CoderEmail && c.Password == hashedPassword);

                if (coder != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, coder.CoderName),
                        new Claim(ClaimTypes.Email, coder.CoderEmail),
                        new Claim(ClaimTypes.Role, coder.AdminCoder ? "Admin" : "User"),
                        new Claim("CoderId", coder.CoderId.ToString())
                    };

                    if (coder.ContestSetter)
                    {
                        claims.Add(new Claim("ContestSetter", "true"));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index");
                }

                ViewBag.Error = "Email hoặc mật khẩu không đúng";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string CoderName, string CoderEmail, string Password, string confirmPassword, bool? Gender, bool ReceiveEmail)
        {
            if (!string.IsNullOrEmpty(CoderName) && !string.IsNullOrEmpty(CoderEmail) &&
                !string.IsNullOrEmpty(Password) && Password == confirmPassword)
            {
                // Chỉ kiểm tra email đã tồn tại
                if (_context.Coders.Any(c => c.CoderEmail == CoderEmail))
                {
                    ViewBag.Error = "Email đã được sử dụng, vui lòng sử dụng email khác";
                    return View();
                }

                var hashedPassword = GetMD5Hash(Password);
                var coder = new Coder
                {
                    CoderName = CoderName,
                    CoderEmail = CoderEmail,
                    Password = hashedPassword,
                    Gender = Gender,
                    ReceiveEmail = ReceiveEmail,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    AdminCoder = false,
                    ContestSetter = false,
                };

                _context.Coders.Add(coder);
                await _context.SaveChangesAsync();

                // Tự động đăng nhập sau khi đăng ký
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, coder.CoderName),
                    new Claim(ClaimTypes.Email, coder.CoderEmail),
                    new Claim(ClaimTypes.Role, "User"),
                    new Claim("CoderId", coder.CoderId.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index");
            }

            ViewBag.Error = "Vui lòng điền đầy đủ thông tin";
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập email";
                return View();
            }

            // Check rate limiting
            if (!await CheckPasswordResetRateLimit(email))
            {
                ViewBag.Error = "Bạn đã yêu cầu đặt lại mật khẩu quá nhiều lần. Vui lòng thử lại sau 1 giờ.";
                return View();
            }

            var user = await _context.Coders.FirstOrDefaultAsync(c => c.CoderEmail == email);
            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại trong hệ thống";
                return View();
            }

            try
            {
                var resetToken = _passwordResetService.GenerateResetToken(user.CoderId);
                var resetLink = Url.Action("ResetPassword", "Home", 
                    new { token = resetToken, id = user.CoderId }, Request.Scheme);

                var emailBody = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #2c3e50;'>Đặt lại mật khẩu ProCoder</h2>
        <p>Xin chào {HttpUtility.HtmlEncode(user.CoderName)},</p>
        <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
        <p>Vui lòng click vào nút bên dưới để đặt lại mật khẩu:</p>
        <p style='text-align: center;'>
            <a href='{resetLink}' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>
                Đặt lại mật khẩu
            </a>
        </p>
        <p>Link này sẽ hết hạn sau 24 giờ.</p>
        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
        <hr style='border: 1px solid #eee; margin: 20px 0;'>
        <p style='color: #7f8c8d; font-size: 12px;'>
            Email này được gửi tự động, vui lòng không trả lời.
            Nếu bạn cần hỗ trợ, hãy liên hệ với chúng tôi qua website.
        </p>
    </div>
</body>
</html>";

                await _emailService.SendEmailAsync(
                    user.CoderEmail,
                    "Đặt lại mật khẩu ProCoder",
                    emailBody,
                    isHtml: true);

                ViewBag.Success = "Chúng tôi đã gửi hướng dẫn đặt lại mật khẩu đến email của bạn. Vui lòng kiểm tra hộp thư.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending password reset email: {ex.Message}");
                ViewBag.Error = "Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau.";
            }

            return View();
        }

        private async Task<bool> CheckPasswordResetRateLimit(string email)
        {
            var now = DateTime.UtcNow;
            var key = email.ToLower();

            if (_resetAttempts.TryGetValue(key, out var attempts))
            {
                if (now.Subtract(attempts.lastAttempt).TotalHours < 1)
                {
                    if (attempts.attempts >= MaxResetAttemptsPerHour)
                    {
                        return false;
                    }
                    _resetAttempts[key] = (attempts.attempts + 1, now);
                }
                else
                {
                    _resetAttempts[key] = (1, now);
                }
            }
            else
            {
                _resetAttempts[key] = (1, now);
            }

            return true;
        }

        public async Task<IActionResult> ResetPassword(string token, int id)
        {
            if (string.IsNullOrEmpty(token) || id == 0)
            {
                return RedirectToAction("Login", new { error = "Link đặt lại mật khẩu không hợp lệ" });
            }

            var isValidToken = _passwordResetService.ValidateResetToken(id, token);
            if (!isValidToken)
            {
                return RedirectToAction("Login", new { error = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn" });
            }

            ViewBag.Token = token;
            ViewBag.Id = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, int id, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(token) || id == 0)
            {
                return RedirectToAction("Login", new { error = "Link đặt lại mật khẩu không hợp lệ" });
            }

            var isValidToken = _passwordResetService.ValidateResetToken(id, token);
            if (!isValidToken)
            {
                return RedirectToAction("Login", new { error = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn" });
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự";
                ViewBag.Token = token;
                ViewBag.Id = id;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu mới không khớp";
                ViewBag.Token = token;
                ViewBag.Id = id;
                return View();
            }

            var user = await _context.Coders.FindAsync(id);
            if (user == null)
            {
                return RedirectToAction("Login", new { error = "Không tìm thấy người dùng" });
            }

            user.Password = GetMD5Hash(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _passwordResetService.RemoveResetToken(id);
            await _context.SaveChangesAsync();

            // Send confirmation email
            try
            {
                var emailBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <h2 style='color: #2c3e50;'>Mật khẩu đã được đặt lại</h2>
                            <p>Xin chào {HttpUtility.HtmlEncode(user.CoderName)},</p>
                            <p>Mật khẩu tài khoản của bạn đã được đặt lại thành công.</p>
                            <p>Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ với chúng tôi ngay lập tức.</p>
                            <hr style='border: 1px solid #eee; margin: 20px 0;'>
                            <p style='color: #7f8c8d; font-size: 12px;'>
                                Email này được gửi tự động, vui lòng không trả lời.
                                Nếu bạn cần hỗ trợ, hãy liên hệ với chúng tôi qua website.
                            </p>
                        </div>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(
                    user.CoderEmail,
                    "Xác nhận đặt lại mật khẩu thành công",
                    emailBody,
                    isHtml: true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending password reset confirmation email: {ex.Message}");
            }

            return RedirectToAction("Login", new { success = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập với mật khẩu mới." });
        }
        public IActionResult HoiDap()
        {
            return View();
        }
    }
}
