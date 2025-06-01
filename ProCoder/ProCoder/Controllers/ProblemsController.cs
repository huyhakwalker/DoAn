using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProCoder.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ProCoder.Controllers
{
    public class ProblemsController : Controller
    {
        private readonly SqlExerciseScoringContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProblemsController> _logger;
        private readonly IConfiguration _configuration;

        public ProblemsController(SqlExerciseScoringContext context, IWebHostEnvironment environment, ILogger<ProblemsController> logger, IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var problems = await _context.Problems
                .Include(p => p.Theme)
                .Include(p => p.Submissions)
                .Include(p => p.Coder)
                .OrderBy(p => p.ProblemId)
                .ToListAsync();
            return View(problems);
        }

        public async Task<IActionResult> Details(int? id, int? contestId = null)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var problem = await _context.Problems
                .Include(p => p.DatabaseSchema)
                .Include(p => p.TestCases)
                    .ThenInclude(t => t.InitData)
                .FirstOrDefaultAsync(m => m.ProblemId == id);

            if (problem == null)
            {
                return NotFound();
            }

            // Nếu có contestId, cần kiểm tra xem người dùng đã đăng ký cuộc thi chưa
            if (contestId.HasValue)
            {
                int coderId = 0;
                // Lấy CoderId từ claims
                var coderIdClaim = User.FindFirst("CoderId");
                if (coderIdClaim != null && int.TryParse(coderIdClaim.Value, out int claimCoderId))
                {
                    coderId = claimCoderId;
                }
                else if (User.Identity.IsAuthenticated)
                {
                    // Lấy từ username
                    var username = User.Identity.Name;
                    var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CoderName == username);
                    if (coder != null)
                    {
                        coderId = coder.CoderId;
                    }
                }

                // Kiểm tra nếu người dùng đã đăng ký cuộc thi này
                var isRegistered = await _context.Participations
                    .AnyAsync(p => p.CoderId == coderId && p.ContestId == contestId.Value);

                if (!isRegistered)
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng ký cuộc thi trước khi làm bài";
                    return RedirectToAction("Details", "Contests", new { id = contestId.Value });
                }

                // Kiểm tra xem bài tập này có thuộc cuộc thi không
                var isInContest = await _context.HasProblems
                    .AnyAsync(hp => hp.ContestId == contestId.Value && hp.ProblemId == id);

                if (!isInContest)
                {
                    TempData["ErrorMessage"] = "Bài tập không thuộc cuộc thi này";
                    return RedirectToAction("Details", "Contests", new { id = contestId.Value });
                }

                // Nếu bài tập thuộc cuộc thi và người dùng đã đăng ký, lưu contestId vào ViewBag
                ViewBag.ContestId = contestId.Value;

                // Tìm thông tin TakePart nếu đã tồn tại
                var participation = await _context.Participations
                    .FirstOrDefaultAsync(p => p.CoderId == coderId && p.ContestId == contestId.Value);

                if (participation != null)
                {
                    var takePart = await _context.TakeParts
                        .FirstOrDefaultAsync(tp => tp.ParticipationId == participation.ParticipationId && tp.ProblemId == id);

                    if (takePart != null)
                    {
                        ViewBag.TakePart = takePart;
                        ViewBag.SubmissionCount = takePart.SubmissionCount;
                        ViewBag.PointWon = takePart.PointWon;
                        ViewBag.IsSolved = takePart.TimeSolved != null;
                    }
                    else
                    {
                        // Nếu chưa có TakePart, đây là lần đầu người dùng mở bài tập này
                        ViewBag.IsFirstAccess = true;
                    }
                }
            }

            try
            {
                // Đọc nội dung của tệp SchemaDefinition
                string schemaContent = "";
                Dictionary<string, List<string>> tableColumns = new Dictionary<string, List<string>>();
                
                if (!string.IsNullOrEmpty(problem.DatabaseSchema?.SchemaDefinitionPath))
                {
                    try
                    {
                        var schemaFilePath = Path.Combine(_environment.WebRootPath, problem.DatabaseSchema.SchemaDefinitionPath.TrimStart('/'));
                        _logger.LogInformation($"Schema file path: {schemaFilePath}");
                        
                        if (System.IO.File.Exists(schemaFilePath))
                        {
                            schemaContent = await System.IO.File.ReadAllTextAsync(schemaFilePath, Encoding.UTF8);
                            
                            // Nếu schema không phải dạng bảng ASCII thì chuyển đổi
                            if (!schemaContent.Contains("+--") && schemaContent.Contains("CREATE TABLE"))
                            {
                                var asciiSchema = ConvertSchemaToAsciiTable(schemaContent);
                                ViewBag.SchemaContent = asciiSchema;
                            }
                            else
                            {
                                ViewBag.SchemaContent = schemaContent;
                            }
                            
                            _logger.LogInformation($"Schema content read successfully, length: {ViewBag.SchemaContent.Length}");
                            
                            // Phân tích schema để lấy thông tin về các cột
                            tableColumns = ExtractColumnsFromSchema(schemaContent);
                        }
                        else
                        {
                            _logger.LogWarning($"Schema file not found: {schemaFilePath}");
                            ViewBag.SchemaContent = $"File not found: {problem.DatabaseSchema.SchemaDefinitionPath}";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error reading schema file: {ex.Message}");
                        ViewBag.SchemaContentError = $"Lỗi khi đọc file schema: {ex.Message}";
                    }
                }

                // Đọc nội dung của các tệp InitData
                ViewBag.InitDataContents = new Dictionary<int, string>();
                var initDataIds = problem.TestCases
                    .Where(tc => tc.InitDataId.HasValue)
                    .Select(tc => tc.InitDataId.Value)
                    .Distinct()
                    .ToList();
                
                _logger.LogInformation($"Found {initDataIds.Count} init data IDs");
                
                var initDatas = await _context.InitData
                    .Where(id => initDataIds.Contains(id.InitDataId))
                    .ToListAsync();
                
                _logger.LogInformation($"Found {initDatas.Count} init data objects");

                foreach (var initData in initDatas)
                {
                    if (!string.IsNullOrEmpty(initData.DataContentPath))
                    {
                        try
                        {
                            var dataFilePath = Path.Combine(_environment.WebRootPath, initData.DataContentPath.TrimStart('/'));
                            _logger.LogInformation($"Init data file path for ID {initData.InitDataId}: {dataFilePath}");
                            
                            if (System.IO.File.Exists(dataFilePath))
                            {
                                var content = await System.IO.File.ReadAllTextAsync(dataFilePath, Encoding.UTF8);
                                
                                // Kiểm tra nếu content là dữ liệu INSERT, convert sang bảng ASCII
                                if (content.Contains("INSERT INTO"))
                                {
                                    content = ConvertInsertStatementsToAsciiTable(content, tableColumns);
                                }
                                
                                ViewBag.InitDataContents[initData.InitDataId] = content;
                                _logger.LogInformation($"Init data content read successfully for ID {initData.InitDataId}, length: {content.Length}");
                            }
                            else
                            {
                                _logger.LogWarning($"Init data file not found: {dataFilePath}");
                                ViewBag.InitDataContents[initData.InitDataId] = $"File not found: {initData.DataContentPath}";
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error reading init data file for ID {initData.InitDataId}: {ex.Message}");
                            ViewBag.InitDataContents[initData.InitDataId] = $"Error reading file: {ex.Message}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in Details action: {ex.Message}");
                ViewBag.ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }

            // Lấy danh sách submission của người dùng hiện tại cho bài tập này (có phân trang)
            try
            {
                int coderId = 0;
                var coderIdClaim = User.FindFirst("CoderId");
                if (coderIdClaim != null && int.TryParse(coderIdClaim.Value, out int claimCoderId))
                {
                    coderId = claimCoderId;
                }
                else if (User.Identity.IsAuthenticated)
                {
                    var username = User.Identity.Name;
                    var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CoderName == username);
                    if (coder != null)
                    {
                        coderId = coder.CoderId;
                    }
                }
                int page = 1;
                int pageSize = 10;
                if (Request.Query.ContainsKey("page"))
                {
                    int.TryParse(Request.Query["page"], out page);
                    if (page < 1) page = 1;
                }
                if (coderId > 0)
                {
                    var query = _context.Submissions
                        .Where(s => s.ProblemId == problem.ProblemId && s.CoderId == coderId)
                        .OrderByDescending(s => s.SubmitTime);
                    int totalSubmissions = await query.CountAsync();
                    var userSubmissions = await query
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
                    problem.Submissions = userSubmissions;
                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalSubmissions = totalSubmissions;
                    ViewBag.TotalPages = (int)Math.Ceiling((double)totalSubmissions / pageSize);
                }
                else
                {
                    problem.Submissions = new List<Submission>();
                    ViewBag.CurrentPage = 1;
                    ViewBag.PageSize = 10;
                    ViewBag.TotalSubmissions = 0;
                    ViewBag.TotalPages = 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading user submissions: {ex.Message}");
                problem.Submissions = new List<Submission>();
                ViewBag.CurrentPage = 1;
                ViewBag.PageSize = 10;
                ViewBag.TotalSubmissions = 0;
                ViewBag.TotalPages = 1;
            }

            return View(problem);
        }

        private string ConvertSchemaToAsciiTable(string schemaContent)
        {
            try
            {
                var result = new StringBuilder();
                
                // Regex để trích xuất tên bảng và định nghĩa cột
                var tableRegex = new Regex(@"CREATE\s+TABLE\s+(\w+)\s*\(([^;]+)\);", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var matches = tableRegex.Matches(schemaContent);
                
                foreach (Match match in matches)
                {
                    if (match.Groups.Count >= 3)
                    {
                        var tableName = match.Groups[1].Value.Trim();
                        var columnsDefinition = match.Groups[2].Value;
                        
                        // Phân tích các dòng định nghĩa cột
                        var columnLines = columnsDefinition.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        var tableData = new List<Dictionary<string, string>>();
                        
                        // Tạo cấu trúc cột chuẩn (loại bỏ cột Constraints)
                        var columns = new List<string> { "Column Name", "Type" };
                        
                        foreach (var line in columnLines)
                        {
                            var trimmedLine = line.Trim();
                            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("PRIMARY KEY") || trimmedLine.StartsWith("FOREIGN KEY"))
                                continue;
                            
                            var parts = trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length < 2)
                                continue;
                            
                            var columnName = parts[0].Trim(',');
                            
                            // Gộp tất cả các phần còn lại như kiểu dữ liệu và ràng buộc
                            var dataType = string.Join(" ", parts.Skip(1));
                            
                            var rowData = new Dictionary<string, string>
                            {
                                { "Column Name", columnName },
                                { "Type", dataType }
                            };
                            
                            tableData.Add(rowData);
                        }
                        
                        // Tạo bảng ASCII
                        result.AppendLine($"Bảng: {tableName}");
                        result.AppendLine(FormatAsPlainAsciiTable(tableData, columns));
                        result.AppendLine();
                    }
                }
                
                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error converting schema to ASCII table: {ex.Message}");
                return schemaContent; // Trả về schema gốc nếu không chuyển đổi được
            }
        }

        private Dictionary<string, List<string>> ExtractColumnsFromSchema(string schemaContent)
        {
            var result = new Dictionary<string, List<string>>();
            
            try
            {
                // Regex để trích xuất tên bảng và định nghĩa cột
                var tableRegex = new Regex(@"CREATE\s+TABLE\s+(\w+)\s*\(([^;]+)\);", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var matches = tableRegex.Matches(schemaContent);
                
                foreach (Match match in matches)
                {
                    if (match.Groups.Count >= 3)
                    {
                        var tableName = match.Groups[1].Value.Trim();
                        var columnDefinitions = match.Groups[2].Value;
                        
                        // Phân tích định nghĩa cột
                        var columns = new List<string>();
                        var columnRegex = new Regex(@"(\w+)\s+\w+", RegexOptions.IgnoreCase);
                        var columnMatches = columnRegex.Matches(columnDefinitions);
                        
                        foreach (Match columnMatch in columnMatches)
                        {
                            if (columnMatch.Groups.Count >= 2)
                            {
                                columns.Add(columnMatch.Groups[1].Value.Trim());
                            }
                        }
                        
                        result[tableName] = columns;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting columns from schema: {ex.Message}");
            }
            
            return result;
        }

        private string ConvertInsertStatementsToAsciiTable(string insertStatements, Dictionary<string, List<string>> tableColumns)
        {
            try
            {
                // Phân tích các câu lệnh INSERT để lấy thông tin bảng và dữ liệu
                var tables = new Dictionary<string, List<Dictionary<string, string>>>();
                var columnsMap = new Dictionary<string, List<string>>();
                
                // Regex cho cả hai định dạng INSERT
                // 1. INSERT INTO Table VALUES (val1, val2), (val3, val4);
                // 2. INSERT INTO Table (col1, col2) VALUES (val1, val2), (val3, val4);
                
                // Định dạng 1: INSERT INTO Table VALUES (val1, val2);
                var regex1 = new Regex(@"INSERT\s+INTO\s+(\w+)\s+VALUES\s+\((.+?)\);", RegexOptions.IgnoreCase);
                
                // Định dạng 2: INSERT INTO Table (col1, col2) VALUES (val1, val2), (val3, val4);
                var regex2 = new Regex(@"INSERT\s+INTO\s+(\w+)\s*\(([^)]+)\)\s*VALUES\s*(.+?);", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                
                // Xử lý định dạng 2 trước (có chỉ định cột)
                var matches2 = regex2.Matches(insertStatements);
                foreach (Match match in matches2)
                {
                    if (match.Groups.Count >= 4)
                    {
                        var tableName = match.Groups[1].Value.Trim();
                        var columnsList = match.Groups[2].Value.Split(',').Select(c => c.Trim()).ToList();
                        var valuesClause = match.Groups[3].Value.Trim();
                        
                        // Tách các bộ giá trị (val1, val2), (val3, val4)
                        var valuesPairs = ExtractValuePairs(valuesClause);
                        
                        if (!tables.ContainsKey(tableName))
                        {
                            tables[tableName] = new List<Dictionary<string, string>>();
                            columnsMap[tableName] = columnsList;
                        }
                        
                        // Thêm từng bộ giá trị vào bảng
                        foreach (var valuesList in valuesPairs)
                        {
                            var rowData = new Dictionary<string, string>();
                            for (int i = 0; i < columnsList.Count && i < valuesList.Count; i++)
                            {
                                rowData[columnsList[i]] = valuesList[i];
                            }
                            tables[tableName].Add(rowData);
                        }
                    }
                }
                
                // Xử lý định dạng 1 (không chỉ định cột)
                var matches1 = regex1.Matches(insertStatements);
                foreach (Match match in matches1)
                {
                    if (match.Groups.Count >= 3 && !regex2.IsMatch(match.Value)) // Tránh trùng lặp với định dạng 2
                    {
                        var tableName = match.Groups[1].Value.Trim();
                        var values = match.Groups[2].Value;
                        
                        // Phân tích giá trị, tách các phần trong ngoặc đơn
                        var valuesList = SplitValues(values);
                        
                        if (!tables.ContainsKey(tableName))
                        {
                            tables[tableName] = new List<Dictionary<string, string>>();
                            
                            // Sử dụng tên cột từ schema nếu có
                            if (tableColumns.ContainsKey(tableName))
                            {
                                columnsMap[tableName] = tableColumns[tableName];
                            }
                            else
                            {
                                // Nếu không có thông tin schema, tạo tên cột mặc định
                                columnsMap[tableName] = new List<string>();
                                for (int i = 0; i < valuesList.Count; i++)
                                {
                                    columnsMap[tableName].Add($"Column {i+1}");
                                }
                            }
                        }
                        
                        // Thêm dữ liệu vào danh sách
                        var rowData = new Dictionary<string, string>();
                        for (int i = 0; i < valuesList.Count && i < columnsMap[tableName].Count; i++)
                        {
                            rowData[columnsMap[tableName][i]] = valuesList[i];
                        }
                        
                        tables[tableName].Add(rowData);
                    }
                }
                
                // Tạo bảng ASCII cho mỗi bảng
                var result = new StringBuilder();
                foreach (var tableName in tables.Keys)
                {
                    result.AppendLine($"Bảng: {tableName}");
                    result.AppendLine(FormatAsPlainAsciiTable(tables[tableName], columnsMap[tableName]));
                    result.AppendLine();
                }
                
                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error converting INSERT statements to ASCII table: {ex.Message}");
                return insertStatements; // Trả về nguyên bản nếu không chuyển đổi được
            }
        }
        
        private List<List<string>> ExtractValuePairs(string valuesClause)
        {
            var result = new List<List<string>>();
            var currentPair = "";
            var parenCount = 0;
            var inQuote = false;
            
            // Duyệt qua chuỗi để phân tích
            for (int i = 0; i < valuesClause.Length; i++)
            {
                char c = valuesClause[i];
                
                if (c == '\'')
                {
                    // Kiểm tra nếu dấu ' không bị escape
                    if (i == 0 || valuesClause[i - 1] != '\\')
                    {
                        inQuote = !inQuote;
                    }
                    currentPair += c;
                }
                else if (c == '(' && !inQuote)
                {
                    parenCount++;
                    if (parenCount == 1)
                    {
                        // Bắt đầu một cặp mới
                        currentPair = "";
                    }
                    else
                    {
                        currentPair += c;
                    }
                }
                else if (c == ')' && !inQuote)
                {
                    parenCount--;
                    if (parenCount == 0)
                    {
                        // Kết thúc một cặp, thêm vào kết quả
                        result.Add(SplitValues(currentPair));
                    }
                    else
                    {
                        currentPair += c;
                    }
                }
                else if (parenCount > 0)
                {
                    // Đang ở trong một cặp ngoặc
                    currentPair += c;
                }
            }
            
            return result;
        }

        private List<string> SplitValues(string values)
        {
            var result = new List<string>();
            var inQuote = false;
            var currentValue = new StringBuilder();
            
            for (int i = 0; i < values.Length; i++)
            {
                char c = values[i];
                
                if (c == '\'' && (i == 0 || values[i-1] != '\\'))
                {
                    inQuote = !inQuote;
                    currentValue.Append(c);
                }
                else if (c == ',' && !inQuote)
                {
                    result.Add(currentValue.ToString().Trim());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }
            
            if (currentValue.Length > 0)
            {
                result.Add(currentValue.ToString().Trim());
            }
            
            return result;
        }

        private string FormatAsPlainAsciiTable(List<Dictionary<string, string>> data, List<string> columns)
        {
            if (data == null || data.Count == 0 || columns == null || columns.Count == 0)
            {
                return "Không có dữ liệu";
            }
            
            // Xác định chiều rộng của mỗi cột
            var colWidths = new Dictionary<string, int>();
            foreach (var col in columns)
            {
                colWidths[col] = col.Length;
            }
            
            foreach (var row in data)
            {
                foreach (var col in columns)
                {
                    if (row.ContainsKey(col))
                    {
                        colWidths[col] = Math.Max(colWidths[col], row[col].Length);
                    }
                }
            }
            
            var sb = new StringBuilder();
            
            // Tạo dòng đầu tiên với dấu +
            sb.Append('+');
            foreach (var col in columns)
            {
                sb.Append(new string('-', colWidths[col] + 2));
                sb.Append('+');
            }
            sb.AppendLine();
            
            // Tạo header
            sb.Append('|');
            foreach (var col in columns)
            {
                sb.Append($" {col.PadRight(colWidths[col])} |");
            }
            sb.AppendLine();
            
            // Tạo dòng phân cách
            sb.Append('+');
            foreach (var col in columns)
            {
                sb.Append(new string('-', colWidths[col] + 2));
                sb.Append('+');
            }
            sb.AppendLine();
            
            // Thêm dữ liệu các dòng
            foreach (var row in data)
            {
                sb.Append('|');
                foreach (var col in columns)
                {
                    string value = row.ContainsKey(col) ? row[col] : "";
                    sb.Append($" {value.PadRight(colWidths[col])} |");
                }
                sb.AppendLine();
            }
            
            // Tạo dòng cuối cùng
            sb.Append('+');
            foreach (var col in columns)
            {
                sb.Append(new string('-', colWidths[col] + 2));
                sb.Append('+');
            }
            
            return sb.ToString();
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int problemId, string query, int? contestId = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { success = false, message = "Please enter an SQL query" });
            }

            try
            {
                var problem = await _context.Problems
                    .Include(p => p.DatabaseSchema)
                    .Include(p => p.TestCases)
                    .FirstOrDefaultAsync(p => p.ProblemId == problemId);

                if (problem == null)
                {
                    return Json(new { success = false, message = "Problem not found" });
                }
                try 
                {
                    var result = await GradeSubmission(problem, query);
                    
                    // Log kết quả để debug
                    _logger.LogInformation($"Grading result: {result.IsCorrect}, Time: {result.ExecutionTime}ms, Score: {result.CorrectTestCases}/{result.TotalTestCases}");
                    
                    // Calculate score based on correct test cases
                    int score = 0;
                    if (result.TotalTestCases > 0)
                    {
                        score = result.TestCaseScore;
                    }
                    
                    // Get current user's CoderId
                    int coderId = 1; // Default to admin if we can't find user
                    
                    // Try to get CoderId from claims
                    var coderIdClaim = User.FindFirst("CoderId");
                    if (coderIdClaim != null && int.TryParse(coderIdClaim.Value, out int claimCoderId))
                    {
                        _logger.LogInformation($"Found CoderId from claim: {claimCoderId}");
                        coderId = claimCoderId;
                    }
                    else if (User.Identity.IsAuthenticated)
                    {
                        // Try to get from username
                        var username = User.Identity.Name;
                        var coder = await _context.Coders.FirstOrDefaultAsync(c => c.CoderName == username);
                        if (coder != null)
                        {
                            _logger.LogInformation($"Found CoderId from username: {coder.CoderId}");
                            coderId = coder.CoderId;
                        }
                        else
                        {
                            _logger.LogWarning("User is authenticated but could not find CoderId");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("User is not authenticated, using default CoderId");
                    }
                    
                    int? takePartID = null;
                    if(contestId != null)
                    {
                        var part = await _context.Participations.FirstOrDefaultAsync(p => p.CoderId == coderId && p.ContestId == contestId);
                        if(part != null)
                        {
                            var hp = await _context.HasProblems
                                .FirstOrDefaultAsync(h => h.ContestId == contestId &&
                                h.ProblemId == problemId);
                            var tp = await _context.TakeParts
                                .FirstOrDefaultAsync(t => t.ParticipationId == part.ParticipationId &&
                                t.ProblemId == problemId);
                            if (tp == null)
                            {
                                tp = new TakePart
                                {
                                    ParticipationId = part.ParticipationId,
                                    ProblemId = problem.ProblemId,
                                    SubmissionCount = 1,
                                    PointWon = result.IsCorrect ? hp.PointProblem : 0,
                                    TimeSolved = result.IsCorrect ? DateTime.Now : null
                                };
                                _context.TakeParts.Add(tp);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                // Tăng số lượt nộp bài
                                tp.SubmissionCount++;
                                
                                // Nếu bài này chưa được giải đúng hoặc điểm chưa tối đa
                                if ((tp.TimeSolved == null || tp.PointWon < hp.PointProblem) && result.IsCorrect)
                                {
                                    tp.PointWon = hp.PointProblem;
                                    tp.TimeSolved = DateTime.Now;
                                }
                                
                                _context.TakeParts.Update(tp);
                                await _context.SaveChangesAsync();
                            }
                            takePartID = tp.TakePartId;
                        }
                    }

                    // Create a new submission
                    var submission = new Submission
                    {
                        ProblemId = problemId,
                        CoderId = coderId,
                        TakePartId = takePartID, // Set to null as requested
                        SubmitTime = DateTime.Now,
                        SubmitCode = query,
                        SubmissionStatus = result.IsCorrect ? "Accepted" : "Wrong Answer",
                        Score = score,
                        ExecutionTime = result.ExecutionTime,
                        ErrorMessage = result.ErrorMessage,
                        CreatedAt = DateTime.Now
                    };
                    
                    _context.Submissions.Add(submission);
                    await _context.SaveChangesAsync(); // Save to get SubmissionId
                    
                    // Associate TestRuns with the created submission
                    foreach (var testRun in result.Item6)
                    {
                        testRun.SubmissionId = submission.SubmissionId;
                        _context.TestRuns.Add(testRun);
                    }
                    
                    // Nếu bài tập được giải đúng, đánh dấu là đã giải
                    if (result.IsCorrect && score >= 100)
                    {
                        _logger.LogInformation($"Solution is correct with score {score}. Checking if already solved...");
                        
                        // Kiểm tra xem đã tồn tại record Solved chưa
                        var existingSolved = await _context.Coders
                            .Where(c => c.CoderId == coderId)
                            .SelectMany(c => c.ProblemsNavigation)
                            .AnyAsync(p => p.ProblemId == problemId);
                            
                        _logger.LogInformation($"Problem already solved check: {existingSolved}");
                            
                        if (!existingSolved)
                        {
                            // Lấy đối tượng Coder và Problem
                            var coder = await _context.Coders
                                .Include(c => c.ProblemsNavigation)
                                .FirstOrDefaultAsync(c => c.CoderId == coderId);
                                
                            var problemToAdd = await _context.Problems
                                .FirstOrDefaultAsync(p => p.ProblemId == problemId);
                                
                            if (coder != null && problemToAdd != null)
                            {
                                // Thêm mối quan hệ many-to-many
                                _logger.LogInformation($"Adding problem {problemId} to coder {coderId} solved list");
                                coder.ProblemsNavigation.Add(problemToAdd);
                                _logger.LogInformation($"Marked problem {problemId} as solved for coder {coderId}");
                            }
                            else
                            {
                                _logger.LogWarning($"Could not find coder ({coderId}) or problem ({problemId}) to mark as solved");
                            }
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Saved changes to database, submission ID: {submission.SubmissionId}");
                    
                    // Trả về kết quả đơn giản cùng với ID của submission mới tạo
                    return Json(new { 
                        ok = true, 
                        pass = result.IsCorrect,
                        time = result.ExecutionTime,
                        error = result.ErrorMessage,
                        score = score,
                        testResults = $"{result.CorrectTestCases}/{result.TotalTestCases}",
                        submissionId = submission.SubmissionId
                    });
                }
                catch (Exception gradingEx)
                {
                    _logger.LogError($"Error in grading: {gradingEx.Message}");
                    _logger.LogError($"Stack trace: {gradingEx.StackTrace}");
                    return Json(new { success = false, message = $"Grading error: {gradingEx.Message}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Submit: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        private async Task<(bool IsCorrect, int ExecutionTime, string ErrorMessage, int CorrectTestCases, int TotalTestCases, List<TestRun> TestRunResults, int TestCaseScore)> GradeSubmission(Problem problem, string userSql)
        {
            try
            {
                // Read schema and initialization data
                string schemaContent = "";
                string answerSql = "";
                
                // Read schema
                if (!string.IsNullOrEmpty(problem.DatabaseSchema?.SchemaDefinitionPath))
                {
                    try
                    {
                        var schemaFilePath = Path.Combine(_environment.WebRootPath, problem.DatabaseSchema.SchemaDefinitionPath.TrimStart('/'));
                        _logger.LogInformation($"Reading schema from path: {schemaFilePath}");
                        
                        if (System.IO.File.Exists(schemaFilePath))
                        {
                            schemaContent = await System.IO.File.ReadAllTextAsync(schemaFilePath, Encoding.UTF8);
                            _logger.LogInformation($"Schema loaded successfully, content length: {schemaContent.Length} bytes");
                        }
                        else
                        {
                            _logger.LogError($"Schema file not found: {schemaFilePath}");
                            return (false, 0, $"Schema file not found at {schemaFilePath}", 0, 0, new List<TestRun>(), 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error reading schema file: {ex.Message}");
                        return (false, 0, $"Error reading schema file: {ex.Message}", 0, 0, new List<TestRun>(), 0);
                    }
                }
                else
                {
                    _logger.LogError("No schema definition path available for problem");
                    return (false, 0, "Schema information not found", 0, 0, new List<TestRun>(), 0);
                }

                // Read answer
                if (!string.IsNullOrEmpty(problem.AnswerQueryPath))
                {
                    try
                    {
                        var answerFilePath = Path.Combine(_environment.WebRootPath, problem.AnswerQueryPath.TrimStart('/'));
                        _logger.LogInformation($"Reading answer file from: {answerFilePath}");
                        
                        if (System.IO.File.Exists(answerFilePath))
                        {
                            answerSql = await System.IO.File.ReadAllTextAsync(answerFilePath, Encoding.UTF8);
                            _logger.LogInformation($"Answer loaded successfully, content length: {answerSql.Length} bytes");
                        }
                        else
                        {
                            _logger.LogError($"Answer file not found: {answerFilePath}");
                            return (false, 0, $"Answer file not found at {answerFilePath}", 0, 0, new List<TestRun>(), 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error reading answer file: {ex.Message}");
                        return (false, 0, $"Error reading answer file: {ex.Message}", 0, 0, new List<TestRun>(), 0);
                    }
                }
                else
                {
                    _logger.LogError("No answer query path available for problem");
                    return (false, 0, "Answer information not found", 0, 0, new List<TestRun>(), 0);
                }

                // Get test cases
                var testCases = problem.TestCases.ToList();
                if (testCases.Count == 0)
                {
                    _logger.LogError("No test cases found for problem");
                    return (false, 0, "No test cases found", 0, 0, new List<TestRun>(), 0);
                }

                _logger.LogInformation($"Found {testCases.Count} test cases");

                // Use connection string to master to be able to create and drop databases
                string connectionString = _configuration.GetConnectionString("SqlConnection") 
                    ?? "Data Source=DESKTOP-QRJ84D1\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False";
                
                // Create unique ID for temporary database
                var dbName = $"PCTest_{Guid.NewGuid().ToString("N").Substring(0, 16)}";
                var executionTime = 0;
                var allTestsCorrect = true;
                var correctTestCases = 0;
                var totalTestCases = testCases.Count;
                var errorMessages = new List<string>();
                var testRunResults = new List<TestRun>(); // Store TestRun objects for each test case
                var totalScore = 0; // Track the total score from test cases

                _logger.LogInformation($"Starting tests with temp DB: {dbName}");

                foreach (var testCase in testCases)
                {
                    // Create a TestRun object
                    var testRun = new TestRun
                    {
                        TestCaseId = testCase.TestCaseId,
                        IsCorrect = false,
                        CreatedAt = DateTime.Now
                    };
                    
                    // Read initialization data for this test case
                    string initDataContent = "";
                    string testcaseFullName = $"p{problem.ProblemId:D3}_testcase{testCase.OrderNumber}";
                    
                    if (testCase.InitDataId.HasValue)
                    {
                        try
                        {
                            var initData = await _context.InitData
                                .FirstOrDefaultAsync(id => id.InitDataId == testCase.InitDataId.Value);

                            if (initData != null && !string.IsNullOrEmpty(initData.DataContentPath))
                            {
                                var dataFilePath = Path.Combine(_environment.WebRootPath, initData.DataContentPath.TrimStart('/'));
                                _logger.LogInformation($"Reading sample data for {testcaseFullName} from: {dataFilePath}");
                                
                                if (System.IO.File.Exists(dataFilePath))
                                {
                                    initDataContent = await System.IO.File.ReadAllTextAsync(dataFilePath, Encoding.UTF8);
                                    _logger.LogInformation($"InitData loaded successfully for {testcaseFullName}, content length: {initDataContent.Length} bytes");
                                }
                                else
                                {
                                    _logger.LogWarning($"InitData file not found for {testcaseFullName}: {dataFilePath}");
                                    errorMessages.Add($"{testcaseFullName}: Sample data file not found at {dataFilePath}");
                                    allTestsCorrect = false;
                                    continue;
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"InitData not found or has no path for {testcaseFullName}, ID: {testCase.InitDataId.Value}");
                                errorMessages.Add($"{testcaseFullName}: Sample data not found or invalid path");
                                allTestsCorrect = false;
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error reading InitData for {testcaseFullName}: {ex.Message}");
                            errorMessages.Add($"{testcaseFullName}: Error reading sample data - {ex.Message}");
                            allTestsCorrect = false;
                            continue;
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"No InitData ID for {testcaseFullName}");
                        errorMessages.Add($"{testcaseFullName}: No sample data ID");
                        allTestsCorrect = false;
                        continue;
                    }

                    // Read expected result for this test case
                    string answerResult = "";
                    if (!string.IsNullOrEmpty(testCase.AnswerResultPath))
                    {
                        try
                        {
                            var answerResultPath = Path.Combine(_environment.WebRootPath, testCase.AnswerResultPath.TrimStart('/'));
                            _logger.LogInformation($"Reading expected result for {testcaseFullName} from: {answerResultPath}");
                            
                            if (System.IO.File.Exists(answerResultPath))
                            {
                                answerResult = await System.IO.File.ReadAllTextAsync(answerResultPath, Encoding.UTF8);
                                _logger.LogInformation($"Answer result loaded successfully for {testcaseFullName}, content length: {answerResult.Length} bytes");
                            }
                            else
                            {
                                _logger.LogWarning($"Answer result file not found for {testcaseFullName}: {answerResultPath}");
                                errorMessages.Add($"{testcaseFullName}: Expected result file not found at {answerResultPath}");
                                allTestsCorrect = false;
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error reading answer result for {testcaseFullName}: {ex.Message}");
                            errorMessages.Add($"{testcaseFullName}: Error reading expected result - {ex.Message}");
                            allTestsCorrect = false;
                            continue;
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"No answer result path for {testcaseFullName}");
                        errorMessages.Add($"{testcaseFullName}: No expected result path");
                        allTestsCorrect = false;
                        continue;
                    }

                    // Execute test case
                    try
                    {
                        _logger.LogInformation($"Running {testcaseFullName}...");
                        using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                        {
                            await connection.OpenAsync();
                            _logger.LogInformation("SQL Connection opened successfully");

                            // Check and delete database if it already exists
                            try
                            {
                                using (var cmd = new System.Data.SqlClient.SqlCommand($"IF DB_ID('{dbName}') IS NOT NULL DROP DATABASE {dbName}", connection))
                                {
                                    await cmd.ExecuteNonQueryAsync();
                                    _logger.LogInformation($"Dropped existing database {dbName} if it existed");
                                }
                            }
                            catch (Exception dropEx)
                            {
                                _logger.LogWarning($"Error checking/dropping existing database: {dropEx.Message}");
                            }

                            // Create temporary database
                            _logger.LogInformation($"Step 1: Create database {dbName}");
                            using (var cmd = new System.Data.SqlClient.SqlCommand($"CREATE DATABASE {dbName}", connection))
                            {
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Use newly created database
                            using (var cmd = new System.Data.SqlClient.SqlCommand($"USE {dbName}", connection))
                            {
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Create schema
                            _logger.LogInformation("Step 2: Create schema, schema content: " + schemaContent.Substring(0, Math.Min(100, schemaContent.Length)) + "...");
                            try
                            {
                                using (var cmd = new System.Data.SqlClient.SqlCommand(schemaContent, connection))
                                {
                                    await cmd.ExecuteNonQueryAsync();
                                    _logger.LogInformation("Schema created successfully");
                                }
                            }
                            catch (Exception schemaEx)
                            {
                                _logger.LogError($"Error creating schema for {testcaseFullName}: {schemaEx.Message}");
                                errorMessages.Add($"{testcaseFullName}: Error creating schema - {schemaEx.Message}");
                                allTestsCorrect = false;
                                
                                // Delete temporary database and continue to next test case
                                try
                                {
                                    using (var cmd = new System.Data.SqlClient.SqlCommand($"USE master; DROP DATABASE IF EXISTS {dbName}", connection))
                                    {
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                                catch {}
                                
                                continue;
                            }

                            // Add sample data
                            _logger.LogInformation("Step 3: Insert sample data, content: " + initDataContent.Substring(0, Math.Min(100, initDataContent.Length)) + "...");
                            try
                            {
                                using (var cmd = new System.Data.SqlClient.SqlCommand(initDataContent, connection))
                                {
                                    await cmd.ExecuteNonQueryAsync();
                                    _logger.LogInformation("Sample data inserted successfully");
                                }
                            }
                            catch (Exception dataEx)
                            {
                                _logger.LogError($"Error inserting sample data for {testcaseFullName}: {dataEx.Message}");
                                errorMessages.Add($"{testcaseFullName}: Error inserting sample data - {dataEx.Message}");
                                allTestsCorrect = false;
                                
                                // Delete temporary database and continue to next test case
                                try
                                {
                                    using (var cmd = new System.Data.SqlClient.SqlCommand($"USE master; DROP DATABASE IF EXISTS {dbName}", connection))
                                    {
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                                catch {}
                                
                                continue;
                            }

                            // Run user query
                            _logger.LogInformation("Step 4: Run user query and convert result to CSV");
                            _logger.LogInformation($"User query: {userSql}");
                            
                            System.Data.DataSet dsUser = new System.Data.DataSet();
                            var startTime = DateTime.Now;
                            try
                            {
                                using (var cmd = new System.Data.SqlClient.SqlCommand(userSql, connection))
                                {
                                    using (var adapter = new System.Data.SqlClient.SqlDataAdapter(cmd))
                                    {
                                        adapter.Fill(dsUser);
                                        _logger.LogInformation($"User query executed, returned {dsUser.Tables.Count} tables");
                                    }
                                }
                            }
                            catch (System.Data.SqlClient.SqlException sqlEx)
                            {
                                _logger.LogError($"Error executing user query for {testcaseFullName}: {sqlEx.Message}");
                                errorMessages.Add($"{testcaseFullName}: SQL syntax error - {sqlEx.Message}");
                                allTestsCorrect = false;
                                
                                // Update TestRun
                                testRun.ErrorMessage = sqlEx.Message;
                                testRun.ExecutionTime = 0;
                                testRunResults.Add(testRun);
                                
                                // Delete temporary database and continue to next test case
                                try
                                {
                                    using (var cmd = new System.Data.SqlClient.SqlCommand($"USE master; DROP DATABASE IF EXISTS {dbName}", connection))
                                    {
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                                catch {}
                                
                                continue;
                            }
                            
                            var currentExecutionTime = (int)(DateTime.Now - startTime).TotalMilliseconds;
                            executionTime = Math.Max(executionTime, currentExecutionTime);
                            _logger.LogInformation($"Query execution time: {currentExecutionTime}ms");
                            
                            // Update TestRun with execution time
                            testRun.ExecutionTime = currentExecutionTime;

                            // Compare results
                            _logger.LogInformation("Step 5: Compare result with expected answer");
                            bool isTestCaseCorrect = false;
                            if (dsUser.Tables.Count > 0)
                            {
                                var csvUser = ExportDataTableToCSV(dsUser.Tables[0]);
                                
                                // Save user output in TestRun
                                testRun.ActualOutput = csvUser;
                                
                                _logger.LogInformation($"User result for {testcaseFullName}:\n{csvUser}");
                                _logger.LogInformation($"Expected result for {testcaseFullName}:\n{answerResult}");
                                
                                isTestCaseCorrect = LineCompareEqual(csvUser, answerResult);
                                _logger.LogInformation($"{testcaseFullName} result: {(isTestCaseCorrect ? "Correct" : "Incorrect")}");
                                
                                // Update TestRun
                                testRun.IsCorrect = isTestCaseCorrect;
                                // Sử dụng điểm từ TestCase cho TestRun nếu đúng
                                testRun.Score = isTestCaseCorrect ? testCase.Score : 0;
                                
                                if (isTestCaseCorrect) {
                                    correctTestCases++;
                                    totalScore += testCase.Score; // Cộng điểm nếu testcase đúng
                                } else {
                                    errorMessages.Add($"{testcaseFullName}: Incorrect result");
                                    allTestsCorrect = false;
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"No results returned from user query for {testcaseFullName}");
                                errorMessages.Add($"{testcaseFullName}: Query returned no results");
                                allTestsCorrect = false;
                                
                                // Update TestRun
                                testRun.ActualOutput = "No results";
                                testRun.ErrorMessage = "Query returned no results";
                            }
                            
                            // Add TestRun to results
                            testRunResults.Add(testRun);

                            // Delete temporary database after completing this test case
                            _logger.LogInformation("Step 6: Delete temporary database");
                            try
                            {
                                using (var cmd = new System.Data.SqlClient.SqlCommand($"USE master; DROP DATABASE IF EXISTS {dbName}", connection))
                                {
                                    await cmd.ExecuteNonQueryAsync();
                                    _logger.LogInformation($"Dropped temporary database {dbName}");
                                }
                            }
                            catch (Exception cleanupEx)
                            {
                                _logger.LogError($"Error dropping database: {cleanupEx.Message}");
                            }
                        }
                    }
                    catch (Exception testEx)
                    {
                        _logger.LogError($"Error running {testcaseFullName}: {testEx.Message}");
                        errorMessages.Add($"{testcaseFullName}: {testEx.Message}");
                        allTestsCorrect = false;
                        
                        // Update TestRun with error
                        testRun.ErrorMessage = testEx.Message;
                        testRunResults.Add(testRun);
                    }
                }

                // Report summary results
                _logger.LogInformation("All test cases completed");
                _logger.LogInformation($"Result: {(allTestsCorrect ? "Correct" : "Incorrect")}");
                _logger.LogInformation($"Correct test cases: {correctTestCases}/{totalTestCases}");
                _logger.LogInformation($"Total score: {totalScore}");
                var allResults = testCases.Select(tc => $"p{problem.ProblemId:D3}_testcase{tc.OrderNumber}: {(errorMessages.Any(em => em.StartsWith($"p{problem.ProblemId:D3}_testcase{tc.OrderNumber}:")) ? "Incorrect" : "Correct")}").ToList();
                _logger.LogInformation($"Problem p{problem.ProblemId:D3} Results: {string.Join(", ", allResults)}");

                return (allTestsCorrect, executionTime, string.Join(". ", errorMessages), correctTestCases, totalTestCases, testRunResults, totalScore);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error grading submission: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                return (false, 0, $"System error: {ex.Message}", 0, 0, new List<TestRun>(), 0);
            }
        }

        // Phương thức xuất DataTable ra CSV (tương tự trong mã mẫu Utils.cs)
        private string ExportDataTableToCSV(System.Data.DataTable dataTable)
        {
            var w = new StringWriter();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                w.Write(dataTable.Columns[i].ColumnName);
                if (i < dataTable.Columns.Count - 1)
                    w.Write(",");
            }
            w.WriteLine();

            foreach (System.Data.DataRow row in dataTable.Rows)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    w.Write(row[i].ToString());
                    if (i < dataTable.Columns.Count - 1)
                        w.Write(",");
                }
                w.WriteLine();
            }
            return w.ToString();
        }

        // Phương thức so sánh hai chuỗi CSV (tương tự trong mã mẫu Utils.cs)
        private bool LineCompareEqual(string line1, string line2)
        {
            line1 = line1.Replace("\r\n", "\n");
            line2 = line2.Replace("\r\n", "\n");

            var arr1 = line1.Split('\n');
            var arr2 = line2.Split('\n');
            var ls1 = arr1.Where(o => o.Trim() != "").ToList();
            var ls2 = arr2.Where(o => o.Trim() != "").ToList();
            if (ls1.Count != ls2.Count) return false;
            for (int i = 0; i < ls1.Count; i++)
            {
                if (ls1[i].Trim() != ls2[i].Trim()) return false;
            }
            return true;
        }    
        private string[] SplitSqlStatements(string sqlScript)
        {
            List<string> statements = new List<string>();
            bool inString = false;
            int startPos = 0;
            
            for (int i = 0; i < sqlScript.Length; i++)
            {
                char c = sqlScript[i];
                
                // Kiểm tra nếu ký tự là dấu nháy đơn không được escape
                if (c == '\'' && (i == 0 || sqlScript[i - 1] != '\\'))
                {
                    inString = !inString;
                }
                // Nếu gặp dấu chấm phẩy và không nằm trong chuỗi
                else if (c == ';' && !inString)
                {
                    string statement = sqlScript.Substring(startPos, i - startPos + 1).Trim();
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        statements.Add(statement);
                    }
                    startPos = i + 1;
                }
            }
            
            // Thêm phần còn lại nếu có
            if (startPos < sqlScript.Length)
            {
                string lastStatement = sqlScript.Substring(startPos).Trim();
                if (!string.IsNullOrWhiteSpace(lastStatement))
                {
                    statements.Add(lastStatement);
                }
            }
            
            return statements.ToArray();
        }

        // Phương thức để phân tích dữ liệu sản phẩm từ chuỗi CSV
        private List<Tuple<string, decimal>> ParseProductsFromCsv(string csv)
        {
            var products = new List<Tuple<string, decimal>>();
            var lines = csv.Split('\n');
            
            // Bỏ qua dòng tiêu đề
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                
                var fields = line.Split(',');
                if (fields.Length >= 2)
                {
                    string productName = fields[0].Trim();
                    if (decimal.TryParse(fields[1].Trim(), out decimal price))
                    {
                        products.Add(new Tuple<string, decimal>(productName, price));
                    }
                }
            }
            
            return products;
        }
    }
}
