using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Text;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DatabaseSchemasAdminController : Controller
    {
        private readonly SqlExerciseScoringContext _context;
        private readonly IWebHostEnvironment _environment;

        public DatabaseSchemasAdminController(SqlExerciseScoringContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var schemas = await _context.DatabaseSchemas
                .Include(s => s.Coder)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(schemas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatabaseSchema databaseSchema)
        {
            if (ModelState.IsValid)
            {
                // Cập nhật các trường thời gian
                databaseSchema.CreatedAt = DateTime.UtcNow;
                databaseSchema.UpdatedAt = DateTime.UtcNow;
                
                // Logic để xử lý file upload nếu có
                
                _context.Add(databaseSchema);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["CoderId"] = new SelectList(_context.Coders, "CoderId", "CoderName", databaseSchema.CoderId);
            return View(databaseSchema);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schema = await _context.DatabaseSchemas.FindAsync(id);
            if (schema == null)
            {
                return NotFound();
            }

            return View(schema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DatabaseSchema schema)
        {
            if (id != schema.DatabaseSchemaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    schema.UpdatedAt = DateTime.UtcNow;
                    _context.Update(schema);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật schema thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.DatabaseSchemas.AnyAsync(e => e.DatabaseSchemaId == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return View(schema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schema = await _context.DatabaseSchemas.FindAsync(id);
            if (schema == null)
            {
                return NotFound();
            }

            _context.DatabaseSchemas.Remove(schema);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa schema thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
} 