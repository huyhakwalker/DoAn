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
            
            var coderName = User.Identity.Name;
            var coder = await _context.Coders
                .FirstOrDefaultAsync(c => c.CoderName == coderName);
            
            return coder?.CoderId;
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
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            // Lấy CoderId trước khi validate ModelState
            var coderId = await GetCurrentCoderId();
            if (!coderId.HasValue)
            {
                return RedirectToAction("Login", "Home");
            }

            // Gán CoderId trước khi validate
            blog.CoderId = coderId.Value;
            blog.BlogDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Blogs.Add(blog);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Đăng bài viết thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi đăng bài. Vui lòng thử lại.");
                }
            }
            else
            {
                // Log ra các lỗi validation để debug
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        // Log hoặc debug error.ErrorMessage
                    }
                }
            }

            return View(blog);
        }

        // Action để hiển thị form chỉnh sửa blog
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

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

            // Kiểm tra quyền chỉnh sửa
            if (!User.IsInRole("Admin") && blog.CoderId != currentCoderId)
            {
                return Forbid();
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

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBlog = await _context.Blogs.FindAsync(id);
                    if (existingBlog == null)
                    {
                        return NotFound();
                    }

                    var currentCoderId = await GetCurrentCoderId();
                    if (!currentCoderId.HasValue)
                    {
                        return RedirectToAction("Login", "Home");
                    }

                    // Kiểm tra quyền chỉnh sửa
                    if (!User.IsInRole("Admin") && existingBlog.CoderId != currentCoderId)
                    {
                        return Forbid();
                    }

                    // Cập nhật các trường có thể sửa
                    existingBlog.BlogTitle = blog.BlogTitle;
                    existingBlog.BlogContent = blog.BlogContent;
                    existingBlog.Published = blog.Published;
                    existingBlog.PinHome = blog.PinHome;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật bài viết thành công!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
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
