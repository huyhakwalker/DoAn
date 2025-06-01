using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize]
    public class DatabaseSchemasAdminController : Controller
    {
        private readonly SqlExerciseScoringContext _context;
        private readonly ILogger<DatabaseSchemasAdminController> _logger;

        public DatabaseSchemasAdminController(SqlExerciseScoringContext context, ILogger<DatabaseSchemasAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = null)
        {
            try
            {
                var query = _context.DatabaseSchemas
                    .Include(d => d.Coder)
                    .OrderByDescending(d => d.CreatedAt)
                    .AsQueryable();
                    
                // Tìm kiếm theo tên hoặc mô tả
                if (!string.IsNullOrEmpty(search))
                {
                    string searchLower = search.Trim().ToLower();
                    query = query.Where(s => s.SchemaName.ToLower().Contains(searchLower)
                                     || (s.Description != null && s.Description.ToLower().Contains(searchLower)));
                }
                
                // Tính toán số lượng trang
                int totalItems = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                
                // Đảm bảo trang hiện tại hợp lệ
                page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

                // Lấy dữ liệu cho trang hiện tại
                var schemas = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Lấy số lượng bài tập cho mỗi schema
                var schemaIds = schemas.Select(s => s.DatabaseSchemaId).ToList();
                var problemCounts = await _context.Problems
                    .Where(p => schemaIds.Contains(p.DatabaseSchemaId))
                    .GroupBy(p => p.DatabaseSchemaId)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());

                // Lấy số lượng init data cho mỗi schema
                var initDataCounts = await _context.InitData
                    .Where(i => schemaIds.Contains(i.DatabaseSchemaId))
                    .GroupBy(i => i.DatabaseSchemaId)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());

                ViewBag.ProblemCounts = problemCounts;
                ViewBag.InitDataCounts = initDataCounts;
                
                // Truyền thông tin phân trang
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.SearchString = search;
                ViewBag.TotalItems = totalItems;

                return View(schemas);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading schemas: {ex.Message}");
                TempData["ErrorMessageSchema"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return View(new List<DatabaseSchema>());
            }
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View(new DatabaseSchema());
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatabaseSchema databaseSchema, string SchemaDefinition)
        {
            try
            {
                var coderName = User.Identity?.Name;
                var coder = await _context.Coders
                    .FirstOrDefaultAsync(c => c.CoderName == coderName);

                if (coder == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Trim các giá trị string
                databaseSchema.SchemaName = databaseSchema.SchemaName?.Trim();
                databaseSchema.Description = databaseSchema.Description?.Trim();

                // Kiểm tra tên schema đã tồn tại chưa
                var existingSchema = await _context.DatabaseSchemas
                    .FirstOrDefaultAsync(d => d.SchemaName == databaseSchema.SchemaName);

                if (existingSchema != null)
                {
                    TempData["ErrorMessageSchema"] = "Tên Schema đã tồn tại trong hệ thống";
                    return View(databaseSchema);
                }

                // Tạo file định nghĩa schema
                if (!string.IsNullOrEmpty(SchemaDefinition))
                {
                    var fileName = $"{databaseSchema.SchemaName.ToLower().Replace(" ", "_")}_schema.csv";
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "Schemas");
                    var filePath = Path.Combine(folderPath, fileName);

                    // Đảm bảo thư mục tồn tại
                    Directory.CreateDirectory(folderPath);

                    // Ghi nội dung vào file
                    await System.IO.File.WriteAllTextAsync(filePath, SchemaDefinition.Trim());

                    // Lưu đường dẫn tương đối vào database
                    databaseSchema.SchemaDefinitionPath = $"/Data/Schemas/{fileName}";
                }
                else
                {
                    TempData["ErrorMessageSchema"] = "Định nghĩa Schema không được để trống";
                    return View(databaseSchema);
                }

                databaseSchema.CoderId = coder.CoderId;
                databaseSchema.CreatedAt = DateTime.UtcNow;
                databaseSchema.UpdatedAt = DateTime.UtcNow;

                _context.Add(databaseSchema);
                await _context.SaveChangesAsync();

                TempData["SuccessMessageSchema"] = "Tạo Database Schema thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageSchema"] = $"Lỗi khi tạo Database Schema: {ex.Message}";
            }

            return View(databaseSchema);
        }

        [HttpGet]
        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var databaseSchema = await _context.DatabaseSchemas
                .Include(d => d.Coder)
                .FirstOrDefaultAsync(d => d.DatabaseSchemaId == id);

            if (databaseSchema == null)
            {
                return NotFound();
            }

            // Đọc nội dung file định nghĩa schema
            try
            {
                if (!string.IsNullOrEmpty(databaseSchema.SchemaDefinitionPath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", databaseSchema.SchemaDefinitionPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        ViewBag.SchemaDefinitionContent = await System.IO.File.ReadAllTextAsync(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading schema definition file: {ex.Message}");
                TempData["ErrorMessageSchema"] = "Không thể đọc file định nghĩa schema";
            }

            ViewBag.Coders = new SelectList(
                await _context.Coders.OrderBy(c => c.CoderName).ToListAsync(),
                "CoderId",
                "CoderName",
                databaseSchema.CoderId
            );

            return View(databaseSchema);
        }

        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DatabaseSchema databaseSchema, string SchemaDefinition)
        {
            if (id != databaseSchema.DatabaseSchemaId)
            {
                return NotFound();
            }

            try
            {
                var existingSchema = await _context.DatabaseSchemas.FirstOrDefaultAsync(d => d.DatabaseSchemaId == id);
                if (existingSchema == null)
                {
                    return NotFound();
                }

                // Kiểm tra tên schema nếu đã bị thay đổi
                if (existingSchema.SchemaName != databaseSchema.SchemaName)
                {
                    var duplicateSchema = await _context.DatabaseSchemas
                        .FirstOrDefaultAsync(d => d.SchemaName == databaseSchema.SchemaName && d.DatabaseSchemaId != id);

                    if (duplicateSchema != null)
                    {
                        TempData["ErrorMessageSchema"] = "Tên Schema đã tồn tại trong hệ thống";

                        ViewBag.Coders = new SelectList(
                            await _context.Coders.OrderBy(c => c.CoderName).ToListAsync(),
                            "CoderId",
                            "CoderName",
                            databaseSchema.CoderId
                        );

                        ViewBag.SchemaDefinitionContent = SchemaDefinition;
                        return View(databaseSchema);
                    }
                }

                // Cập nhật file định nghĩa schema nếu có thay đổi
                if (!string.IsNullOrEmpty(SchemaDefinition))
                {
                    // Nếu tên schema thay đổi, tạo file mới và xoá file cũ
                    string fileName;
                    string oldFilePath = null;

                    if (existingSchema.SchemaName != databaseSchema.SchemaName)
                    {
                        // Lưu đường dẫn file cũ để xoá sau
                        if (!string.IsNullOrEmpty(existingSchema.SchemaDefinitionPath))
                        {
                            oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingSchema.SchemaDefinitionPath.TrimStart('/'));
                        }

                        // Tạo tên file mới
                        fileName = $"{databaseSchema.SchemaName.ToLower().Replace(" ", "_")}_schema.csv";
                    }
                    else
                    {
                        // Giữ nguyên tên file
                        fileName = Path.GetFileName(existingSchema.SchemaDefinitionPath);
                    }

                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "Schemas");
                    var newFilePath = Path.Combine(folderPath, fileName);

                    // Đảm bảo thư mục tồn tại
                    Directory.CreateDirectory(folderPath);

                    // Ghi nội dung mới vào file
                    await System.IO.File.WriteAllTextAsync(newFilePath, SchemaDefinition.Trim());

                    // Xoá file cũ nếu khác tên
                    if (oldFilePath != null && oldFilePath != newFilePath && System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                    // Cập nhật đường dẫn mới
                    existingSchema.SchemaDefinitionPath = $"/Data/Schemas/{fileName}";
                }

                // Cập nhật các trường
                existingSchema.SchemaName = databaseSchema.SchemaName?.Trim();
                existingSchema.Description = databaseSchema.Description?.Trim();
                existingSchema.CoderId = databaseSchema.CoderId;
                existingSchema.UpdatedAt = DateTime.UtcNow;

                _context.Update(existingSchema);
                await _context.SaveChangesAsync();

                TempData["SuccessMessageSchema"] = "Cập nhật Database Schema thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessageSchema"] = $"Lỗi khi cập nhật: {ex.Message}";
            }

            ViewBag.Coders = new SelectList(
                await _context.Coders.OrderBy(c => c.CoderName).ToListAsync(),
                "CoderId",
                "CoderName",
                databaseSchema.CoderId
            );

            ViewBag.SchemaDefinitionContent = SchemaDefinition;
            return View(databaseSchema);
        }

        [HttpGet]
        [Route("Detail/{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var databaseSchema = await _context.DatabaseSchemas
                .Include(d => d.Coder)
                .Include(d => d.InitData)
                .Include(d => d.Problems)
                .FirstOrDefaultAsync(d => d.DatabaseSchemaId == id);

            if (databaseSchema == null)
            {
                return NotFound();
            }

            // Đọc nội dung file định nghĩa schema
            try
            {
                if (!string.IsNullOrEmpty(databaseSchema.SchemaDefinitionPath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", databaseSchema.SchemaDefinitionPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        ViewBag.SchemaDefinitionContent = await System.IO.File.ReadAllTextAsync(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading schema definition file: {ex.Message}");
                TempData["ErrorMessageSchema"] = "Không thể đọc file định nghĩa schema";
            }

            return View(databaseSchema);
        }

        [HttpPost]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var databaseSchema = await _context.DatabaseSchemas
                .Include(d => d.Problems)
                .Include(d => d.InitData)
                .FirstOrDefaultAsync(d => d.DatabaseSchemaId == id);

            if (databaseSchema == null)
            {
                return NotFound();
            }

            try
            {
                // Kiểm tra xem schema có đang được sử dụng không
                if (databaseSchema.Problems.Any())
                {
                    TempData["ErrorMessageSchema"] = "Không thể xóa Database Schema đang được sử dụng trong các bài tập";
                    return RedirectToAction(nameof(Index));
                }

                // Xóa các initData liên quan
                foreach (var initData in databaseSchema.InitData)
                {
                    // Xóa file dữ liệu của initData nếu tồn tại
                    if (!string.IsNullOrEmpty(initData.DataContentPath))
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", initData.DataContentPath.TrimStart('/'));
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }

                    _context.InitData.Remove(initData);
                }

                // Xóa file định nghĩa schema nếu tồn tại
                if (!string.IsNullOrEmpty(databaseSchema.SchemaDefinitionPath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", databaseSchema.SchemaDefinitionPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                // Xóa database schema
                _context.DatabaseSchemas.Remove(databaseSchema);
                await _context.SaveChangesAsync();

                TempData["SuccessMessageSchema"] = "Xóa Database Schema và các file liên quan thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi xóa Database Schema: {ex.Message}");
                TempData["ErrorMessageSchema"] = $"Lỗi khi xóa Database Schema: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}