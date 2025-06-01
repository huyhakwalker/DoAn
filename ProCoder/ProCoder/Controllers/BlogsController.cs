using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using System;
using System.Security.Claims;

namespace ProCoder.Controllers
{
    public class BlogsController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public BlogsController(SqlExerciseScoringContext context)
        {
            _context = context;
        }

        // Helper method để lấy CoderId từ user hiện tại
        private async Task<int?> GetCurrentCoderId()
        {
            if (!User.Identity.IsAuthenticated) return null;
            
            // Thử lấy CoderId từ claim
            var coderIdClaim = User.FindFirst("CoderId");
            if (coderIdClaim != null && int.TryParse(coderIdClaim.Value, out int coderId))
            {
                Console.WriteLine($"Tìm thấy CoderId từ claim: {coderId}");
                return coderId;
            }
            
            // Backup plan: lấy từ username
            var coderName = User.Identity.Name;
            var coder = await _context.Coders
                .FirstOrDefaultAsync(c => c.CoderName == coderName);
            
            if (coder != null)
            {
                Console.WriteLine($"Tìm thấy CoderId từ database: {coder.CoderId}");
                return coder.CoderId;
            }
            
            Console.WriteLine("Không tìm thấy CoderId");
            return null;
        }

        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Blogs
                .Include(b => b.Coder)
                .Include(b => b.Comments)
                    .ThenInclude(c => c.Coder)
                .FirstOrDefaultAsync(b => b.BlogId == id);

            if (blog == null)
            {
                return NotFound();
            }

            // Chỉ admin và tác giả mới có thể xem bài chưa công bố
            if (!blog.Published)
            {
                var currentCoderId = await GetCurrentCoderId();
                var isAdmin = User.IsInRole("Admin");
                
                if (!isAdmin && blog.CoderId != currentCoderId)
                {
                    return NotFound("Bài viết này chưa được công bố");
                }
            }

            return View(blog);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int blogId, string commentContent)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            // Lấy CoderId từ UserName thay vì NameIdentifier
            var coderName = User.Identity.Name;
            var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CoderName == coderName);
            
            if (coder == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var comment = new Comment
            {
                BlogId = blogId,
                CoderId = coder.CoderId,
                CommentContent = commentContent,
                CommentDate = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = blogId });
        }

        // Thêm action để hiển thị form tạo blog mới
        [HttpGet]
        public IActionResult Create()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        // Action xử lý việc tạo blog mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BlogTitle,BlogContent,Published,PinHome")] Blog blog)
        {
            try
            {
                // Log để debug
                Console.WriteLine($"Create POST - BlogTitle: {blog.BlogTitle}");
                Console.WriteLine($"Published: {blog.Published}, PinHome: {blog.PinHome}");
                
                if (!User.Identity.IsAuthenticated)
                {
                    Console.WriteLine("User không đăng nhập");
                    TempData["Error"] = "Bạn cần đăng nhập để tạo bài viết.";
                    return RedirectToAction("Login", "Home");
                }

                // Lấy CoderId từ claim
                var coderIdClaim = User.FindFirst("CoderId");
                if (coderIdClaim != null && int.TryParse(coderIdClaim.Value, out int coderId))
                {
                    Console.WriteLine($"Found CoderId from claim: {coderId}");
                    blog.CoderId = coderId;
                }
                else
                {
                    // Backup plan: lấy từ username
                    var coderName = User.Identity.Name;
                    Console.WriteLine($"User identity name: {coderName}");
                    
                    if (string.IsNullOrEmpty(coderName))
                    {
                        Console.WriteLine("Tên người dùng trống");
                        TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                        return View(blog);
                    }
                    
                    var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CoderName == coderName);
                    if (coder == null)
                    {
                        Console.WriteLine($"Không tìm thấy coder với tên: {coderName}");
                        TempData["Error"] = "Không tìm thấy thông tin người dùng trong hệ thống.";
                        return View(blog);
                    }
                    
                    blog.CoderId = coder.CoderId;
                }
                
                // Tạo blog mới trực tiếp thay vì dùng model binding
                var newBlog = new Blog
                {
                    BlogTitle = blog.BlogTitle,
                    BlogContent = blog.BlogContent,
                    BlogDate = DateTime.Now,
                    CoderId = blog.CoderId,
                    Published = blog.Published,
                    PinHome = blog.PinHome
                };
                
                Console.WriteLine($"Tạo blog mới với CoderId: {newBlog.CoderId}");
                
                // Lưu blog vào database
                _context.Blogs.Add(newBlog);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Đã lưu blog với ID: {newBlog.BlogId}");
                TempData["Success"] = "Tạo bài viết thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                TempData["Error"] = $"Lỗi khi tạo bài viết: {ex.Message}";
                return View(blog);
            }
        }

        // Action để hiển thị form chỉnh sửa blog
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Console.WriteLine($"Edit GET - Blog ID: {id}");

            var blog = await _context.Blogs
                .Include(b => b.Coder)
                .FirstOrDefaultAsync(m => m.BlogId == id);
                
            if (blog == null)
            {
                return NotFound();
            }

            var currentCoderId = await GetCurrentCoderId();
            if (!currentCoderId.HasValue)
            {
                Console.WriteLine("Không tìm thấy CoderId người dùng hiện tại");
                TempData["Error"] = "Bạn cần đăng nhập để chỉnh sửa bài viết.";
                return RedirectToAction("Login", "Home");
            }

            Console.WriteLine($"CoderId người dùng hiện tại: {currentCoderId}, CoderId của blog: {blog.CoderId}");

            // Kiểm tra quyền chỉnh sửa
            if (!User.IsInRole("Admin") && blog.CoderId != currentCoderId)
            {
                Console.WriteLine("Không có quyền chỉnh sửa");
                TempData["Error"] = "Bạn không có quyền chỉnh sửa bài viết này.";
                return RedirectToAction(nameof(Index));
            }

            return View(blog);
        }

        // Action xử lý việc cập nhật blog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BlogId,BlogTitle,BlogContent,Published,PinHome")] Blog blog)
        {
            if (id != blog.BlogId)
            {
                return NotFound();
            }

            Console.WriteLine($"Edit POST - Blog ID: {id}");
            Console.WriteLine($"BlogTitle: {blog.BlogTitle}, Published: {blog.Published}, PinHome: {blog.PinHome}");

            try
            {
                var existingBlog = await _context.Blogs
                    .Include(b => b.Coder)
                    .FirstOrDefaultAsync(b => b.BlogId == id);
                    
                if (existingBlog == null)
                {
                    Console.WriteLine("Blog không tồn tại");
                    return NotFound();
                }

                var currentCoderId = await GetCurrentCoderId();
                if (!currentCoderId.HasValue)
                {
                    Console.WriteLine("User chưa đăng nhập");
                    TempData["Error"] = "Bạn cần đăng nhập để chỉnh sửa bài viết.";
                    return RedirectToAction("Login", "Home");
                }

                Console.WriteLine($"CoderId người dùng hiện tại: {currentCoderId}, CoderId của blog: {existingBlog.CoderId}");

                // Kiểm tra quyền chỉnh sửa
                if (!User.IsInRole("Admin") && existingBlog.CoderId != currentCoderId)
                {
                    Console.WriteLine("Không có quyền chỉnh sửa");
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa bài viết này.";
                    return RedirectToAction(nameof(Index));
                }

                // Cập nhật các trường được phép chỉnh sửa
                existingBlog.BlogTitle = blog.BlogTitle;
                existingBlog.BlogContent = blog.BlogContent;
                
                // Xác định giá trị boolean từ form
                bool isPublished = Request.Form["Published"] == "true" || Request.Form["Published"] == "on";
                bool isPinned = Request.Form["PinHome"] == "true" || Request.Form["PinHome"] == "on";
                
                existingBlog.Published = isPublished;
                existingBlog.PinHome = isPinned;
                
                Console.WriteLine($"Đã cập nhật: Published={isPublished}, PinHome={isPinned}");

                await _context.SaveChangesAsync();
                Console.WriteLine("Lưu thay đổi thành công");
                
                TempData["Success"] = "Cập nhật bài viết thành công!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                TempData["Error"] = $"Lỗi khi cập nhật bài viết: {ex.Message}";
            }
            
            return View(blog);
        }

        // Helper method để kiểm tra blog tồn tại
        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.BlogId == id);
        }

        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs
                .Include(b => b.Coder)
                .OrderByDescending(b => b.BlogDate) // Sắp xếp theo thời gian mới nhất
                .ToListAsync();

            return View(blogs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            var currentCoderId = await GetCurrentCoderId();
            if (!currentCoderId.HasValue)
            {
                return RedirectToAction("Login", "Home");
            }

            // Kiểm tra quyền xóa
            if (!User.IsInRole("Admin") && blog.CoderId != currentCoderId)
            {
                return Forbid();
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa bài viết thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
