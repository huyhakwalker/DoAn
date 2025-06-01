using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize]
    public class InitDataAdminController : Controller
    {
        private readonly SqlExerciseScoringContext _context;
        private readonly ILogger<InitDataAdminController> _logger;

        public InitDataAdminController(SqlExerciseScoringContext context, ILogger<InitDataAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create(int databaseSchemaId)
        {
            var initData = new InitData { DatabaseSchemaId = databaseSchemaId };
            return View(initData);
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DatabaseSchemaId,DataName,DataContentPath,Description")] InitData initData, string DataContent)
        {
            if (ModelState.IsValid)
            {
                // Tạo file dữ liệu
                if (!string.IsNullOrEmpty(DataContent))
                {
                    var fileName = $"{initData.DataName.ToLower().Replace(" ", "_")}.csv";
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "InitData");
                    var filePath = Path.Combine(folderPath, fileName);

                    // Đảm bảo thư mục tồn tại
                    Directory.CreateDirectory(folderPath);

                    // Ghi nội dung vào file
                    await System.IO.File.WriteAllTextAsync(filePath, DataContent.Trim());

                    // Lưu đường dẫn tương đối vào database
                    initData.DataContentPath = $"/Data/InitData/{fileName}";
                }
                else
                {
                    // Trả về lỗi nếu không có nội dung dữ liệu
                    ModelState.AddModelError("DataContent", "Nội dung dữ liệu là bắt buộc");
                    return View(initData);
                }
                
                initData.CreatedAt = DateTime.UtcNow;
                initData.UpdatedAt = DateTime.UtcNow;

                _context.Add(initData);
                await _context.SaveChangesAsync();
                TempData["SuccessMessageInitData"] = "Tạo InitData thành công!";
                return RedirectToAction("Detail", "DatabaseSchemasAdmin", new { area = "Admin", id = initData.DatabaseSchemaId });
            }
            return View(initData);
        }

        [HttpGet]
        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var initData = await _context.InitData
                .Include(i => i.DatabaseSchema)
                .FirstOrDefaultAsync(i => i.InitDataId == id);
                
            if (initData == null)
            {
                return NotFound();
            }
            
            // Đọc nội dung file dữ liệu
            try
            {
                if (!string.IsNullOrEmpty(initData.DataContentPath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", initData.DataContentPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        ViewBag.DataContent = await System.IO.File.ReadAllTextAsync(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading init data file: {ex.Message}");
            }
            
            return View(initData);
        }

        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InitData initData, string DataContent)
        {
            if (id != initData.InitDataId)
            {
                return NotFound();
            }

            // Xóa ModelState lỗi cho DataContentPath vì chúng ta sẽ tự thiết lập nó
            ModelState.Remove("DataContentPath");

            if (!string.IsNullOrEmpty(DataContent))
            {
                // Chúng ta có nội dung, tiếp tục xử lý
                ModelState.Remove("DataContent");
            }
            else
            {
                // Báo lỗi nếu không có nội dung
                ModelState.AddModelError("DataContent", "Nội dung dữ liệu là bắt buộc");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingInitData = await _context.InitData.FindAsync(id);
                    if (existingInitData == null)
                    {
                        return NotFound();
                    }
                    
                    // Cập nhật file dữ liệu nếu có thay đổi
                    if (!string.IsNullOrEmpty(DataContent))
                    {
                        string filePath;
                        
                        // Nếu đã có đường dẫn file, sử dụng lại
                        if (!string.IsNullOrEmpty(existingInitData.DataContentPath))
                        {
                            filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                                existingInitData.DataContentPath.TrimStart('/'));
                        }
                        // Nếu chưa có, tạo file mới
                        else
                        {
                            var fileName = $"{initData.DataName.ToLower().Replace(" ", "_")}.csv";
                            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "InitData");
                            
                            // Đảm bảo thư mục tồn tại
                            Directory.CreateDirectory(folderPath);
                            
                            filePath = Path.Combine(folderPath, fileName);
                            existingInitData.DataContentPath = $"/Data/InitData/{fileName}";
                        }
                        
                        // Ghi nội dung vào file
                        await System.IO.File.WriteAllTextAsync(filePath, DataContent.Trim());
                    }
                    
                    // Cập nhật các trường
                    existingInitData.DataName = initData.DataName;
                    existingInitData.Description = initData.Description;
                    existingInitData.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Update(existingInitData);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessageInitData"] = "Cập nhật InitData thành công!";
                    return RedirectToAction("Detail", "DatabaseSchemasAdmin", new { area = "Admin", id = existingInitData.DatabaseSchemaId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InitDataExists(initData.InitDataId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(initData);
        }

        [HttpGet]
        [Route("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var initData = await _context.InitData
                .Include(i => i.DatabaseSchema)
                .FirstOrDefaultAsync(i => i.InitDataId == id);

            if (initData == null)
            {
                return NotFound();
            }
            
            // Đọc nội dung file dữ liệu
            try
            {
                if (!string.IsNullOrEmpty(initData.DataContentPath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", initData.DataContentPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        ViewBag.DataContent = await System.IO.File.ReadAllTextAsync(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading init data file: {ex.Message}");
            }

            return View(initData);
        }

        [HttpPost]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var initData = await _context.InitData.FindAsync(id);
            if (initData == null)
            {
                return NotFound();
            }
            
            // Xóa file dữ liệu nếu tồn tại
            try
            {
                if (!string.IsNullOrEmpty(initData.DataContentPath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", initData.DataContentPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting init data file: {ex.Message}");
            }

            _context.InitData.Remove(initData);
            await _context.SaveChangesAsync();

            TempData["SuccessMessageInitData"] = "Xóa InitData thành công!";
            return RedirectToAction("Detail", "DatabaseSchemasAdmin", new { area = "Admin", id = initData.DatabaseSchemaId });
        }
        
        private bool InitDataExists(int id)
        {
            return _context.InitData.Any(e => e.InitDataId == id);
        }
    }
} 