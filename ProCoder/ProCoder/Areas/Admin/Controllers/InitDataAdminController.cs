using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize]
    public class InitDataAdminController : Controller
    {
        private readonly SqlExerciseScoringContext _context;

        public InitDataAdminController(SqlExerciseScoringContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create([Bind("DatabaseSchemaId,DataName,DataContent,Description")] InitData initData)
        {
            if (ModelState.IsValid)
            {
                _context.Add(initData);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo InitData thành công!";
                return RedirectToAction("Details", "DatabaseSchemasAdmin", new { id = initData.DatabaseSchemaId });
            }
            return View(initData);
        }

        [HttpGet]
        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var initData = await _context.InitData.FindAsync(id);
            if (initData == null)
            {
                return NotFound();
            }
            return View(initData);
        }

        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InitData initData)
        {
            if (id != initData.InitDataId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(initData);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật InitData thành công!";
                return RedirectToAction("Details", "DatabaseSchemasAdmin", new { id = initData.DatabaseSchemaId });
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

            _context.InitData.Remove(initData);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa InitData thành công!";
            return RedirectToAction("Details", "DatabaseSchemasAdmin", new { id = initData.DatabaseSchemaId });
        }
    }
} 