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
    [Route("Admin/[controller]")]
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
        [HttpGet]
        [Route("Create")]
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
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestCase testCase, string AnswerResult)
        {
            // Xóa validation errors cho các trường chúng ta sẽ điền sau
            ModelState.Remove("AnswerResultPath");
            ModelState.Remove("AnswerResultContent");
            ModelState.Remove("Problem");

            if (string.IsNullOrEmpty(AnswerResult))
            {
                ModelState.AddModelError("AnswerResult", "Kết quả mong đợi là bắt buộc");
            }

            // Kiểm tra AnswerResult có dữ liệu hợp lệ
            if (!string.IsNullOrEmpty(AnswerResult))
            {
                var lines = AnswerResult.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
                if (lines.Count == 0)
                {
                    ModelState.AddModelError("AnswerResult", "Kết quả mong đợi phải có ít nhất một dòng dữ liệu");
                }
            }

            // Kiểm tra OrderNumber
            if (testCase.OrderNumber <= 0)
            {
                ModelState.AddModelError("OrderNumber", "Thứ tự phải lớn hơn 0");
            }

            // Kiểm tra Score
            if (testCase.Score < 0)
            {
                ModelState.AddModelError("Score", "Điểm phải lớn hơn hoặc bằng 0");
            }

            // Lấy thông tin Problem
            var problem = await _context.Problems
                .Include(p => p.DatabaseSchema)
                .FirstOrDefaultAsync(p => p.ProblemId == testCase.ProblemId);

            if (problem == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin bài tập");
                return View(testCase);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lưu nội dung từ textarea vào file
                    var resultsPath = Path.Combine(_environment.WebRootPath, "Data", "AnswerResults");
                    Directory.CreateDirectory(resultsPath);

                    // Tạo tên file theo mẫu trong SQL
                    var problemCode = problem.ProblemCode.ToLower();
                    
                    // Đếm số lượng test case hiện có
                    int testCaseCount = await _context.TestCases
                        .Where(tc => tc.ProblemId == testCase.ProblemId)
                        .CountAsync();
                    
                    var fileName = $"{problemCode}_testcase{testCaseCount + 1}.csv";
                    var filePath = Path.Combine(resultsPath, fileName);

                    // Ghi nội dung vào file
                    await System.IO.File.WriteAllTextAsync(filePath, AnswerResult.Trim());

                    // Lưu đường dẫn tương đối vào database
                    testCase.AnswerResultPath = $"/Data/AnswerResults/{fileName}";
                    testCase.AnswerResultContent = AnswerResult.Trim();
                    
                    // Thiết lập thời gian
                    testCase.CreatedAt = DateTime.UtcNow;
                    testCase.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Add(testCase);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Tạo Test Case thành công!";
                    return RedirectToAction("Details", "ProblemsAdmin", new { area = "Admin", id = testCase.ProblemId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi tạo Test Case: {ex.Message}");
                }
            }

            // Load lại các options cho dropdown
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
        [HttpGet]
        [Route("Edit/{id:int}")]
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

            // Đọc nội dung file CSV
            try
            {
                if (!string.IsNullOrEmpty(testCase.AnswerResultPath))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, testCase.AnswerResultPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        ViewBag.AnswerResult = await System.IO.File.ReadAllTextAsync(filePath);
                    }
                    else
                    {
                        ViewBag.AnswerResult = "Không tìm thấy file kết quả";
                        ModelState.AddModelError("AnswerResult", "Không tìm thấy file kết quả");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.AnswerResult = "";
                ModelState.AddModelError("AnswerResult", $"Lỗi khi đọc file: {ex.Message}");
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
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TestCase testCase, string AnswerResult)
        {
            if (id != testCase.TestCaseId)
            {
                return NotFound();
            }

            // Xóa validation errors cho các trường sẽ được điền sau
            ModelState.Remove("AnswerResultPath");
            ModelState.Remove("AnswerResultContent");
            ModelState.Remove("Problem");

            // Kiểm tra AnswerResult
            if (string.IsNullOrEmpty(AnswerResult))
            {
                ModelState.AddModelError("AnswerResult", "Kết quả mong đợi là bắt buộc");
            }

            // Lấy thông tin Problem trước
            var problem = await _context.Problems
                .Include(p => p.DatabaseSchema)
                .FirstOrDefaultAsync(p => p.ProblemId == testCase.ProblemId);

            if (problem == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin bài tập");
                
                ViewBag.Problem = null;
                ViewBag.AnswerResult = AnswerResult;
                return View(testCase);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin TestCase từ database để giữ nguyên các trường không thay đổi
                    var existingTestCase = await _context.TestCases
                        .AsNoTracking()
                        .FirstOrDefaultAsync(tc => tc.TestCaseId == id);

                    if (existingTestCase == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật file kết quả nếu có nội dung
                    if (!string.IsNullOrEmpty(AnswerResult))
                    {
                        // Tạo thư mục lưu trữ kết quả nếu chưa tồn tại
                        var resultsPath = Path.Combine(_environment.WebRootPath, "Data", "AnswerResults");
                        Directory.CreateDirectory(resultsPath);

                        // Xử lý đặt tên file theo mẫu SQL
                        string problemCode = problem.ProblemCode.ToLower();
                        
                        // Lấy số thứ tự từ file hiện tại nếu có
                        int testCaseNumber;
                        string currentPath = existingTestCase.AnswerResultPath;
                        
                        if (currentPath != null && currentPath.Contains("_testcase"))
                        {
                            // Lấy số từ định dạng xxx_testcaseN.csv
                            int startIndex = currentPath.IndexOf("_testcase") + 9;
                            int endIndex = currentPath.IndexOf(".csv");
                            
                            if (startIndex > 0 && endIndex > startIndex)
                            {
                                string numberPart = currentPath.Substring(startIndex, endIndex - startIndex);
                                if (int.TryParse(numberPart, out testCaseNumber))
                                {
                                    // Giữ nguyên số thứ tự
                                }
                                else
                                {
                                    testCaseNumber = testCase.OrderNumber;
                                }
                            }
                            else
                            {
                                testCaseNumber = testCase.OrderNumber;
                            }
                        }
                        else
                        {
                            testCaseNumber = testCase.OrderNumber;
                        }
                        
                        // Tạo tên file mới
                        var fileName = $"{problemCode}_testcase{testCaseNumber}.csv";
                        var filePath = Path.Combine(resultsPath, fileName);
                        
                        // Ghi nội dung vào file
                        await System.IO.File.WriteAllTextAsync(filePath, AnswerResult.Trim());
                        
                        // Cập nhật đường dẫn tương đối
                        testCase.AnswerResultPath = $"/Data/AnswerResults/{fileName}";
                        testCase.AnswerResultContent = AnswerResult;
                    }
                    else
                    {
                        // Giữ nguyên đường dẫn cũ nếu không có dữ liệu mới
                        testCase.AnswerResultPath = existingTestCase.AnswerResultPath;
                    }

                    // Gán lại CreatedAt từ đối tượng cũ
                    testCase.CreatedAt = existingTestCase.CreatedAt;
                    
                    // Gán ngày cập nhật
                    testCase.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Entry(testCase).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Cập nhật Test Case thành công!";
                    return RedirectToAction("Details", "ProblemsAdmin", new { area = "Admin", id = testCase.ProblemId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật: {ex.Message}");
                    TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
                }
            }

            // Load lại các options cho dropdown
            ViewBag.InitDatas = await _context.InitData
                .Where(id => id.DatabaseSchemaId == problem.DatabaseSchemaId)
                .Select(id => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = id.InitDataId.ToString(),
                    Text = id.DataName
                })
                .ToListAsync();

            ViewBag.Problem = problem;
            ViewBag.AnswerResult = AnswerResult;
            return View(testCase);
        }

        // GET: Admin/TestCasesAdmin/Details/5
        [HttpGet]
        [Route("Details/{id:int}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testCase = await _context.TestCases
                .Include(tc => tc.Problem)
                    .ThenInclude(p => p.DatabaseSchema)
                .Include(tc => tc.InitData)
                    .ThenInclude(i => i.DatabaseSchema)
                .FirstOrDefaultAsync(tc => tc.TestCaseId == id);

            if (testCase == null)
            {
                return NotFound();
            }

            // Đọc nội dung file kết quả
            try
            {
                if (!string.IsNullOrEmpty(testCase.AnswerResultPath))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, testCase.AnswerResultPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        testCase.AnswerResultContent = await System.IO.File.ReadAllTextAsync(filePath);
                    }
                    else
                    {
                        testCase.AnswerResultContent = "Không tìm thấy file kết quả";
                    }
                }
            }
            catch (Exception ex)
            {
                testCase.AnswerResultContent = $"Lỗi khi đọc file: {ex.Message}";
            }

            return View(testCase);
        }

        // POST: Admin/TestCasesAdmin/Delete/5
        [HttpPost]
        [Route("Delete/{id:int}")]
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