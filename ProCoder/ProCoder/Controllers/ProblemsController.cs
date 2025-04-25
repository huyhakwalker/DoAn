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

        public async Task<IActionResult> Details(int? id)
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
        public async Task<IActionResult> Submit(int problemId, string query)
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
                    _logger.LogInformation($"Grading result: {result.IsCorrect}, Time: {result.ExecutionTime}ms");
                    
                    // Trả về kết quả đơn giản, chỉ hiển thị trong console
                    return Json(new { 
                        ok = true, 
                        pass = result.IsCorrect,
                        time = result.ExecutionTime,
                        error = result.ErrorMessage
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

        private async Task<(bool IsCorrect, int ExecutionTime, string ErrorMessage)> GradeSubmission(Problem problem, string userSql)
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
                            return (false, 0, $"Schema file not found at {schemaFilePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error reading schema file: {ex.Message}");
                        return (false, 0, $"Error reading schema file: {ex.Message}");
                    }
                }
                else
                {
                    _logger.LogError("No schema definition path available for problem");
                    return (false, 0, "Schema information not found");
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
                            return (false, 0, $"Answer file not found at {answerFilePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error reading answer file: {ex.Message}");
                        return (false, 0, $"Error reading answer file: {ex.Message}");
                    }
                }
                else
                {
                    _logger.LogError("No answer query path available for problem");
                    return (false, 0, "Answer information not found");
                }

                // Get test cases
                var testCases = problem.TestCases.ToList();
                if (testCases.Count == 0)
                {
                    _logger.LogError("No test cases found for problem");
                    return (false, 0, "No test cases found");
                }

                _logger.LogInformation($"Found {testCases.Count} test cases");

                // Use connection string to master to be able to create and drop databases
                string connectionString = _configuration.GetConnectionString("SqlConnection") 
                    ?? "Data Source=DESKTOP-QRJ84D1\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False";
                
                // Create unique ID for temporary database
                var dbName = $"PCTest_{Guid.NewGuid().ToString("N").Substring(0, 16)}";
                var executionTime = 0;
                var allTestsCorrect = true;
                var errorMessages = new List<string>();

                _logger.LogInformation($"Starting tests with temp DB: {dbName}");

                foreach (var testCase in testCases)
                {
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

                            // Compare results
                            _logger.LogInformation("Step 5: Compare result with expected answer");
                            bool isTestCaseCorrect = false;
                            if (dsUser.Tables.Count > 0)
                            {
                                var csvUser = ExportDataTableToCSV(dsUser.Tables[0]);
                                
                                _logger.LogInformation($"User result for {testcaseFullName}:\n{csvUser}");
                                _logger.LogInformation($"Expected result for {testcaseFullName}:\n{answerResult}");
                                
                                isTestCaseCorrect = LineCompareEqual(csvUser, answerResult);
                                _logger.LogInformation($"{testcaseFullName} result: {(isTestCaseCorrect ? "Correct" : "Incorrect")}");
                                
                                if (!isTestCaseCorrect)
                                {
                                    errorMessages.Add($"{testcaseFullName}: Incorrect result");
                                    allTestsCorrect = false;
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"No results returned from user query for {testcaseFullName}");
                                errorMessages.Add($"{testcaseFullName}: Query returned no results");
                                allTestsCorrect = false;
                            }

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
                    }
                }

                // Report summary results
                _logger.LogInformation("All test cases completed");
                _logger.LogInformation($"Result: {(allTestsCorrect ? "Correct" : "Incorrect")}");
                var allResults = testCases.Select(tc => $"p{problem.ProblemId:D3}_testcase{tc.OrderNumber}: {(errorMessages.Any(em => em.StartsWith($"p{problem.ProblemId:D3}_testcase{tc.OrderNumber}:")) ? "Incorrect" : "Correct")}").ToList();
                _logger.LogInformation($"Problem p{problem.ProblemId:D3} Results: {string.Join(", ", allResults)}");

                return (allTestsCorrect, executionTime, string.Join(". ", errorMessages));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error grading submission: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                return (false, 0, $"System error: {ex.Message}");
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

        [HttpPost]
        public IActionResult SaveQueryToCsv(int problemId, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { success = false, message = "No query content to save" });
            }

            try
            {
                // Ensure directory exists
                string userQueriesDir = Path.Combine(_environment.ContentRootPath, "wwwroot", "Data", "UserQuerys");
                if (!Directory.Exists(userQueriesDir))
                {
                    Directory.CreateDirectory(userQueriesDir);
                    _logger.LogInformation($"Created directory: {userQueriesDir}");
                }

                // Create filename with problemId and timestamp to ensure uniqueness
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileName = $"problem_{problemId}_{timestamp}.csv";
                string filePath = Path.Combine(userQueriesDir, fileName);

                // Save user query content directly without adding headers or CSV formatting
                System.IO.File.WriteAllText(filePath, query, Encoding.UTF8);
                _logger.LogInformation($"Saved query to file: {filePath}");

                return Json(new { success = true, filePath = filePath });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving query to file: {ex.Message}");
                return Json(new { success = false, message = $"Error saving query: {ex.Message}" });
            }
        }

        // Helper method để tách các câu lệnh SQL
        private string[] SplitSqlStatements(string sqlScript)
        {
            // Sử dụng regex để tách các câu lệnh SQL ngăn cách bởi dấu chấm phẩy
            // nhưng bỏ qua dấu chấm phẩy trong chuỗi (nằm trong dấu nháy đơn)
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
