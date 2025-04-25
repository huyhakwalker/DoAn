using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ProCoder.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class TestCasesAdminController : Controller
    {
        private readonly SqlExerciseScoringContext _context;
        private readonly IWebHostEnvironment _environment;

        public TestCasesAdminController(SqlExerciseScoringContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/TestCasesAdmin/Create
        public async Task<IActionResult> Create(int problemId)
        {
            var problem = await _context.Problems
                .Include(p => p.DatabaseSchema)
                .FirstOrDefaultAsync(p => p.ProblemId == problemId);

            if (problem == null)
            {
                return NotFound();
            }

            // Get init data list
            ViewBag.InitDatas = await _context.InitData
                .Where(id => id.DatabaseSchemaId == problem.DatabaseSchemaId)
                .Select(id => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = id.InitDataId.ToString(),
                    Text = id.DataName
                })
                .ToListAsync();

            ViewBag.Problem = problem;

            var testCase = new TestCase
            {
                ProblemId = problemId,
                OrderNumber = 1,
                Score = 10
            };

            return View(testCase);
        }

        // POST: Admin/TestCasesAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestCase testCase, IFormFile answerResultFile)
        {
            if (ModelState.IsValid)
            {
                if (answerResultFile != null && answerResultFile.Length > 0)
                {
                    // Save file to appropriate path
                    var resultsPath = Path.Combine(_environment.WebRootPath, "Data", "AnswerResults");
                    Directory.CreateDirectory(resultsPath);

                    // Create unique filename
                    var fileName = $"testcase_{testCase.ProblemId}_{DateTime.Now:yyyyMMddHHmmss}.csv";
                    var filePath = Path.Combine(resultsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await answerResultFile.CopyToAsync(stream);
                    }

                    // Save relative path to database
                    testCase.AnswerResultPath = $"/Data/AnswerResults/{fileName}";
                }
                else
                {
                    ModelState.AddModelError("", "Vui lòng chọn file kết quả");
                    return View(testCase);
                }

                testCase.CreatedAt = DateTime.UtcNow;
                testCase.UpdatedAt = DateTime.UtcNow;
                
                _context.Add(testCase);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "ProblemsAdmin", new { area = "Admin", id = testCase.ProblemId });
            }

            var problem = await _context.Problems
                .Include(p => p.DatabaseSchema)
                .FirstOrDefaultAsync(p => p.ProblemId == testCase.ProblemId);

            ViewBag.InitDatas = await _context.InitData
                .Where(id => id.DatabaseSchemaId == problem.DatabaseSchemaId)
                .Select(id => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = id.InitDataId.ToString(),
                    Text = id.DataName
                })
                .ToListAsync();

            ViewBag.Problem = problem;
            return View(testCase);
        }

        // GET: Admin/TestCasesAdmin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testCase = await _context.TestCases
                .Include(tc => tc.Problem)
                    .ThenInclude(p => p.DatabaseSchema)
                .FirstOrDefaultAsync(tc => tc.TestCaseId == id);

            if (testCase == null)
            {
                return NotFound();
            }

            ViewBag.InitDatas = await _context.InitData
                .Where(id => id.DatabaseSchemaId == testCase.Problem.DatabaseSchemaId)
                .Select(id => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = id.InitDataId.ToString(),
                    Text = id.DataName
                })
                .ToListAsync();

            ViewBag.Problem = testCase.Problem;
            return View(testCase);
        }

        // POST: Admin/TestCasesAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TestCase testCase, IFormFile answerResultFile)
        {
            if (id != testCase.TestCaseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (answerResultFile != null && answerResultFile.Length > 0)
                    {
                        // Save file to appropriate path
                        var resultsPath = Path.Combine(_environment.WebRootPath, "Data", "AnswerResults");
                        Directory.CreateDirectory(resultsPath);

                        // Create unique filename
                        var fileName = $"testcase_{testCase.ProblemId}_{DateTime.Now:yyyyMMddHHmmss}.csv";
                        var filePath = Path.Combine(resultsPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await answerResultFile.CopyToAsync(stream);
                        }

                        // Save relative path to database
                        testCase.AnswerResultPath = $"/Data/AnswerResults/{fileName}";
                    }

                    testCase.UpdatedAt = DateTime.UtcNow;
                    _context.Update(testCase);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Details", "ProblemsAdmin", new { area = "Admin", id = testCase.ProblemId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.TestCases.AnyAsync(e => e.TestCaseId == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            var problem = await _context.Problems
                .Include(p => p.DatabaseSchema)
                .FirstOrDefaultAsync(p => p.ProblemId == testCase.ProblemId);

            ViewBag.InitDatas = await _context.InitData
                .Where(id => id.DatabaseSchemaId == problem.DatabaseSchemaId)
                .Select(id => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = id.InitDataId.ToString(),
                    Text = id.DataName
                })
                .ToListAsync();

            ViewBag.Problem = problem;
            return View(testCase);
        }

        // POST: Admin/TestCasesAdmin/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var testCase = await _context.TestCases.FindAsync(id);
            if (testCase == null)
            {
                return NotFound();
            }

            _context.TestCases.Remove(testCase);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "ProblemsAdmin", new { area = "Admin", id = testCase.ProblemId });
        }
    }
} 