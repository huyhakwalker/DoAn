using Microsoft.AspNetCore.Mvc;
using ProCoder.Models;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class BlogsAdminController : Controller
    {
        private readonly SqlExerciseScoringContext db = new SqlExerciseScoringContext();

        [Route("")]
        [Route("Index")]
        [HttpGet]
        public IActionResult Index()
        {
            var blogs = db.Blogs
                          .Include(b => b.Coder)
                          .ToList();
            return View(blogs);
        }

        [Route("Create")]
        [HttpGet]
        public IActionResult Create()
        {
            // Lấy danh sách tất cả người dùng từ database
            var coders = db.Coders.ToList();
            
            // Truyền danh sách người dùng vào ViewBag để hiển thị trong dropdown
            ViewBag.Coders = coders;
            
            // Tạo đối tượng Blog mới
            var blog = new Blog();
            
            return View(blog);
        }

        [Route("Create")]
        [HttpPost]
        public IActionResult Create(Blog blog)
        {
            if (ModelState.IsValid)
            {
                blog.BlogDate = DateTime.UtcNow;

                // Lưu vào cơ sở dữ liệu
                db.Blogs.Add(blog);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            // Nếu ModelState không hợp lệ, lấy lại danh sách người dùng để hiển thị dropdown
            ViewBag.Coders = db.Coders.ToList();
            return View(blog);
        }

        [Route("Delete/{id:int}")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var blog = db.Blogs.FirstOrDefault(b => b.BlogId == id);
                if (blog == null)
                {
                    return NotFound();
                }

                // Xóa các comments liên quan
                var comments = db.Comments.Where(c => c.BlogId == id);
                db.Comments.RemoveRange(comments);

                // Xóa blog
                db.Blogs.Remove(blog);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting blog: {ex.ToString()}");
                TempData["ErrorMessage"] = "Không thể xóa bài viết này. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [Route("Detail/{id:int}")]
        [HttpGet]
        public IActionResult Detail(int id)
        {
            var blog = db.Blogs
                         .Include(b => b.Coder)
                         .FirstOrDefault(b => b.BlogId == id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }

        [Route("Edit/{id:int}")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var blog = db.Blogs
                         .Include(b => b.Coder)
                         .FirstOrDefault(b => b.BlogId == id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }

        [Route("Edit/{id:int}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Blog blog)
        {
            if (id != blog.BlogId)
            {
                return NotFound();
            }

            try
            {
                var existingBlog = db.Blogs.FirstOrDefault(b => b.BlogId == id);
                if (existingBlog == null)
                {
                    return NotFound();
                }

                // Cập nhật các thông tin khác
                existingBlog.BlogTitle = blog.BlogTitle;
                existingBlog.BlogContent = blog.BlogContent;
                existingBlog.BlogDate = blog.BlogDate;
                existingBlog.Published = blog.Published;
                existingBlog.PinHome = blog.PinHome;

                db.Update(existingBlog);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
            }

            return View(blog);
        }
    }
}
