using Microsoft.AspNetCore.Mvc;
using ProCoder.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models.ViewModels;

namespace ProCoder.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SqlExerciseScoringContext _context;

        public HomeController(ILogger<HomeController> logger, SqlExerciseScoringContext context)
        {
            _logger = logger;
            _context = context;

            // Tạo tài khoản mặc định nếu chưa tồn tại
            CreateDefaultAccounts();
        }

        private void CreateDefaultAccounts()
        {
            // Tạo tài khoản user nếu chưa có
            if (!_context.Coders.Any(c => c.CoderName == "codertest"))
            {
                var user = new Coder
                {
                    CoderName = "codertest",
                    CoderEmail = "codertest@example.com",
                    Password = "train123",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    AdminCoder = false,
                    ContestSetter = false,
                    ReceiveEmail = true
                };
                _context.Coders.Add(user);
            }

            // Tạo tài khoản admin nếu chưa có
            if (!_context.Coders.Any(c => c.CoderName == "admintest"))
            {
                var admin = new Coder
                {
                    CoderName = "admintest",
                    CoderEmail = "admintest@example.com",
                    Password = "train123",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    AdminCoder = true,
                    ContestSetter = true,
                    ReceiveEmail = true
                };
                _context.Coders.Add(admin);
            }

            // Lưu thay đổi vào database
            _context.SaveChanges();
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                PinnedBlogs = await _context.Blogs
                    .Where(b => b.PinHome && b.Published)
                    .OrderByDescending(b => b.BlogDate)
                    .Take(5)
                    .Include(b => b.Coder)
                    .ToListAsync(),

                TopCoders = await _context.Coders
                    .OrderByDescending(c => c.Submissions.Count)
                    .Take(5)
                    .Select(c => new TopCoderViewModel 
                    { 
                        CoderName = c.CoderName,
                        SubmissionCount = c.Submissions.Count 
                    })
                    .ToListAsync()
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
                var coder = await _context.Coders
                    .FirstOrDefaultAsync(c => c.CoderEmail == CoderEmail && c.Password == Password);

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
                // Kiểm tra username và email đã tồn tại
                if (_context.Coders.Any(c => c.CoderName == CoderName))
                {
                    ViewBag.Error = "Tên đăng nhập đã tồn tại";
                    return View();
                }

                if (_context.Coders.Any(c => c.CoderEmail == CoderEmail))
                {
                    ViewBag.Error = "Email đã được sử dụng";
                    return View();
                }

                var coder = new Coder
                {
                    CoderName = CoderName,
                    CoderEmail = CoderEmail,
                    Password = Password,
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
    }
}
