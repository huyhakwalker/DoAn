using Microsoft.AspNetCore.Mvc;
using ProCoder.Models;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize]
    public class BlogsAdminController : Controller
    {
        private readonly SqlExerciseScoringContext _context;
        private readonly ILogger<BlogsAdminController> _logger;

        public BlogsAdminController(SqlExerciseScoringContext context, ILogger<BlogsAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Helper method để lấy thông tin người dùng đang đăng nhập
        private async Task<Coder> GetCurrentCoderAsync()
        {
            var coderName = User.Identity?.Name;
            if (!string.IsNullOrEmpty(coderName))
            {
                var coder = await _context.Coders
                    .FirstOrDefaultAsync(c => c.CoderName == coderName);
                return coder;
            }
            return null;
        }

        [Route("")]
        [Route("Index")]
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = null)
        {
            try
            {
                var query = _context.Blogs
                    .Include(b => b.Coder)
                    .AsQueryable();
                    
                // Tìm kiếm theo tiêu đề blog
                if (!string.IsNullOrEmpty(search))
                {
                    string searchLower = search.Trim().ToLower();
                    query = query.Where(b => b.BlogTitle.ToLower().Contains(searchLower));
                }
                
                // Tính toán số lượng trang
                int totalItems = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                
                // Đảm bảo trang hiện tại hợp lệ
                page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

                // Lấy dữ liệu cho trang hiện tại
                var blogs = await query
                    .OrderByDescending(b => b.BlogDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                    
                // Truyền thông tin phân trang
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.SearchString = search;
                ViewBag.TotalItems = totalItems;
                
                return View(blogs);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading blogs: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return View(new List<Blog>());
            }
        }

        [Route("Create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try 
            {
                // Lấy thông tin người dùng hiện tại
                var currentCoder = await GetCurrentCoderAsync();

                if (currentCoder == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("Index");
                }

                // Tạo bài viết mới với CoderId được set sẵn
                var blog = new Blog
                {
                    CoderId = currentCoder.CoderId,
                    BlogDate = DateTime.Now,
                    Published = false,
                    PinHome = false
                };

                // Lấy danh sách tất cả người dùng từ database
                var coders = await _context.Coders.ToListAsync();
                
                // Truyền thông tin vào ViewBag
                ViewBag.Coders = coders;
                ViewBag.CurrentCoder = currentCoder;
                
                return View(blog);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Create GET: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog)
        {
            try
            {
                // Log tất cả các giá trị nhận được từ form
                foreach (var key in Request.Form.Keys)
                {
                    _logger.LogInformation($"Form key: {key}, value: {Request.Form[key]}");
                }

                // Tạo blog mới và set giá trị trực tiếp từ form
                var newBlog = new Blog();
                
                // Thiết lập các giá trị cần thiết
                newBlog.BlogTitle = Request.Form["BlogTitle"].ToString();
                newBlog.BlogContent = Request.Form["BlogContent"].ToString();
                
                // Lấy CoderId - ưu tiên từ form, nếu không có thì lấy từ người dùng hiện tại
                if (int.TryParse(Request.Form["CoderId"], out int coderId) && coderId > 0)
                {
                    newBlog.CoderId = coderId;
                    _logger.LogInformation($"Sử dụng CoderId từ form: {coderId}");
                }
                else
                {
                    var currentCoder = await GetCurrentCoderAsync();
                    if (currentCoder != null)
                    {
                        newBlog.CoderId = currentCoder.CoderId;
                        _logger.LogInformation($"Sử dụng CoderId từ người dùng hiện tại: {currentCoder.CoderId}");
                    }
                    else
                    {
                        _logger.LogWarning("Không tìm thấy thông tin người dùng");
                        TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                        ViewBag.Coders = await _context.Coders.ToListAsync();
                        ViewBag.CurrentCoder = null;
                        return View(blog);
                    }
                }
                
                // Xử lý boolean fields
                newBlog.Published = Request.Form["Published"] == "true";
                newBlog.PinHome = Request.Form["PinHome"] == "true";
                newBlog.BlogDate = DateTime.UtcNow;
                
                _logger.LogInformation($"Chuẩn bị lưu blog - Title: {newBlog.BlogTitle}, CoderId: {newBlog.CoderId}, Published: {newBlog.Published}, PinHome: {newBlog.PinHome}");

                // Validate thủ công
                bool isValid = true;
                
                if (string.IsNullOrEmpty(newBlog.BlogTitle))
                {
                    ModelState.AddModelError("BlogTitle", "Tiêu đề không được để trống");
                    isValid = false;
                    _logger.LogWarning("Tiêu đề không được để trống");
                }
                
                if (string.IsNullOrEmpty(newBlog.BlogContent))
                {
                    ModelState.AddModelError("BlogContent", "Nội dung không được để trống");
                    isValid = false;
                    _logger.LogWarning("Nội dung không được để trống");
                }
                
                if (newBlog.CoderId <= 0)
                {
                    ModelState.AddModelError("CoderId", "Người viết không hợp lệ");
                    isValid = false;
                    _logger.LogWarning("Người viết không hợp lệ");
                }

                if (isValid)
                {
                    // Kiểm tra xem Coder có tồn tại không
                    var coder = await _context.Coders.FindAsync(newBlog.CoderId);
                    if (coder == null)
                    {
                        _logger.LogWarning($"Không tìm thấy Coder với ID: {newBlog.CoderId}");
                        TempData["ErrorMessage"] = $"Không tìm thấy người dùng với ID: {newBlog.CoderId}";
                        ViewBag.Coders = await _context.Coders.ToListAsync();
                        ViewBag.CurrentCoder = await GetCurrentCoderAsync();
                        return View(newBlog);
                    }

                    try
                    {
                        // Thử lưu vào database
                        _context.Blogs.Add(newBlog);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation($"Đã tạo blog mới với ID: {newBlog.BlogId}");
                        TempData["SuccessMessage"] = "Tạo mới bài viết thành công!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError($"Lỗi database khi lưu blog: {dbEx.Message}");
                        if (dbEx.InnerException != null)
                        {
                            _logger.LogError($"Inner exception: {dbEx.InnerException.Message}");
                        }
                        TempData["ErrorMessage"] = $"Lỗi khi lưu bài viết: {dbEx.Message}";
                    }
                }
                else
                {
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                    }
                    
                    TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin nhập vào.";
                }
                
                // Nếu có lỗi, hiển thị lại form với dữ liệu đã nhập
                ViewBag.Coders = await _context.Coders.ToListAsync();
                ViewBag.CurrentCoder = await GetCurrentCoderAsync();
                return View(newBlog);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi không xác định khi tạo blog: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = $"Lỗi không xác định: {ex.Message}";
                
                ViewBag.Coders = await _context.Coders.ToListAsync();
                ViewBag.CurrentCoder = await GetCurrentCoderAsync();
                return View(blog);
            }
        }

        [Route("Delete/{id:int}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.BlogId == id);
                if (blog == null)
                {
                    return NotFound();
                }

                // Xóa các comments liên quan
                var comments = await _context.Comments.Where(c => c.BlogId == id).ToListAsync();
                _context.Comments.RemoveRange(comments);

                // Xóa blog
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa bài viết thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting blog: {ex.Message}");
                TempData["ErrorMessage"] = "Không thể xóa bài viết này. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [Route("Detail/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var blog = await _context.Blogs
                           .Include(b => b.Coder)
                           .FirstOrDefaultAsync(b => b.BlogId == id);
                if (blog == null)
                {
                    return NotFound();
                }
                return View(blog);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting blog details: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi tải thông tin bài viết: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [Route("Edit/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var blog = await _context.Blogs
                           .Include(b => b.Coder)
                           .FirstOrDefaultAsync(b => b.BlogId == id);
                if (blog == null)
                {
                    return NotFound();
                }
                
                ViewBag.Coders = await _context.Coders.ToListAsync();
                ViewBag.CurrentCoder = blog.Coder;
                return View(blog);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Edit GET: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi tải thông tin bài viết: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [Route("Edit/{id:int}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog)
        {
            if (id != blog.BlogId)
            {
                return NotFound();
            }

            try
            {
                // Log các giá trị
                _logger.LogInformation($"Cập nhật blog - ID: {blog.BlogId}, Title: {blog.BlogTitle}");
                _logger.LogInformation($"Form values - Published: {Request.Form["Published"]}, PinHome: {Request.Form["PinHome"]}");
                
                var existingBlog = await _context.Blogs
                    .Include(b => b.Coder)
                    .FirstOrDefaultAsync(b => b.BlogId == id);
                    
                if (existingBlog == null)
                {
                    return NotFound();
                }

                // Cập nhật các thông tin
                existingBlog.BlogTitle = blog.BlogTitle;
                existingBlog.BlogContent = blog.BlogContent;
                
                // Xử lý boolean fields
                existingBlog.Published = Request.Form["Published"] == "true";
                existingBlog.PinHome = Request.Form["PinHome"] == "true";

                // Giữ nguyên ngày tạo và CoderId
                // existingBlog.BlogDate = blog.BlogDate; // Không cần thiết, giữ nguyên giá trị cũ
                
                _logger.LogInformation($"Đã parse Published: {existingBlog.Published}, PinHome: {existingBlog.PinHome}");

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Edit POST: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
            }

            ViewBag.Coders = await _context.Coders.ToListAsync();
            var currentCoder = await _context.Coders.FindAsync(blog.CoderId);
            ViewBag.CurrentCoder = currentCoder;
            return View(blog);
        }
    }
}
