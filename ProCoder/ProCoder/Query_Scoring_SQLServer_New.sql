--USE master;
--GO
--DROP DATABASE IF EXISTS sql_exercise_scoring;
--GO
CREATE DATABASE sql_exercise_scoring;
GO
USE sql_exercise_scoring;
GO

-- Tạo bảng Coder trước vì nhiều bảng khác tham chiếu tới
CREATE TABLE Coder (
    CoderId INT IDENTITY(1,1) PRIMARY KEY,
    CoderName NVARCHAR(255) NOT NULL,
    CoderEmail NVARCHAR(255) NOT NULL UNIQUE,
    CoderAvatar NVARCHAR(255),
    Password NVARCHAR(255) NOT NULL,
    ReceiveEmail BIT DEFAULT 1,
    Gender BIT,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    CreatedBy VARCHAR(36),
    UpdatedBy VARCHAR(36),
    AdminCoder BIT NOT NULL DEFAULT 0,
    ContestSetter BIT NOT NULL DEFAULT 0
);

-- Sửa bảng DatabaseSchema, thêm CoderID và bỏ trường InitialData
CREATE TABLE DatabaseSchema (
    DatabaseSchemaId INT IDENTITY(1,1) PRIMARY KEY,
    SchemaName NVARCHAR(255) NOT NULL UNIQUE,
    SchemaDefinitionPath NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    CoderId INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL
);

-- Tạo bảng InitData mới (đổi tên từ SchemaData)
CREATE TABLE InitData (
    InitDataId INT IDENTITY(1,1) PRIMARY KEY,
    DatabaseSchemaId INT NOT NULL,
    DataName NVARCHAR(255) NOT NULL,
    DataContentPath NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    CONSTRAINT FK_InitData_DatabaseSchema FOREIGN KEY (DatabaseSchemaId) 
        REFERENCES DatabaseSchema(DatabaseSchemaId) ON DELETE CASCADE
);

CREATE TABLE Problem (
    ProblemId INT IDENTITY(1,1) PRIMARY KEY,
    ProblemCode NVARCHAR(255) NOT NULL UNIQUE,
    ProblemName NVARCHAR(255) NOT NULL,
    ProblemDescription NVARCHAR(MAX) NOT NULL,
    ProblemExplanation NVARCHAR(MAX),
    AnswerQueryPath NVARCHAR(255) NOT NULL,
    Published BIT NOT NULL DEFAULT 0,
    CoderId INT NOT NULL,
    DatabaseSchemaId INT NOT NULL,
    ThemeId INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL
);

CREATE TABLE Blog (
    BlogId INT IDENTITY(1,1) PRIMARY KEY,
    BlogTitle NVARCHAR(255) NOT NULL,
    BlogContent NVARCHAR(MAX) NOT NULL,
    BlogDate DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    Published BIT NOT NULL DEFAULT 0,
    CoderId INT NOT NULL,
    PinHome BIT NOT NULL DEFAULT 0
);

CREATE TABLE Comments (
    CommentId INT IDENTITY(1,1) PRIMARY KEY,
    BlogId INT NOT NULL,
    CoderId INT NOT NULL,
    CommentDate DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    CommentContent NVARCHAR(MAX) NOT NULL
);

CREATE TABLE Favourite (
    CoderId INT NOT NULL,
    ProblemId INT NOT NULL,
    Note NVARCHAR(MAX),
    PRIMARY KEY (CoderId, ProblemId)
);

CREATE TABLE Solved (
    CoderId INT NOT NULL,
    ProblemId INT NOT NULL,
    PRIMARY KEY (CoderId, ProblemId)
);

CREATE TABLE Contest (
    ContestId INT IDENTITY(1,1) PRIMARY KEY,
    CoderId INT NOT NULL,
    ContestName NVARCHAR(255) NOT NULL,
    ContestDescription NVARCHAR(MAX) NOT NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    RuleType NVARCHAR(255) NOT NULL CHECK (RuleType IN ('ACM', 'IOI', 'Custom')),
    FailedPenalty INT NOT NULL DEFAULT 0,
    Published BIT NOT NULL DEFAULT 0,
    StatusContest NVARCHAR(50) NOT NULL CHECK (StatusContest IN ('Pending', 'Running', 'Finished')),
    Duration INT NOT NULL,
    RankingFinished BIT NOT NULL DEFAULT 0,
    FrozenTime DATETIME2
);

CREATE TABLE Participation (
    ParticipationId INT IDENTITY(1,1) PRIMARY KEY,
    CoderId INT NOT NULL,
    ContestId INT NOT NULL,
    RegisterTime DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    PointScore INT NOT NULL DEFAULT 0,
    TimeScore INT NOT NULL DEFAULT 0,
    Ranking INT NOT NULL DEFAULT 0,
    SolvedCount INT NOT NULL DEFAULT 0,
    RegisterMac NVARCHAR(255)
);

CREATE TABLE TakePart (
    TakePartId INT IDENTITY(1,1) PRIMARY KEY,
    ParticipationId INT NOT NULL,
    ProblemId INT NOT NULL,
    TimeSolved DATETIME2,
    PointWon INT NOT NULL DEFAULT 0,
    MaxPoint INT NOT NULL,
    SubmissionCount INT NOT NULL DEFAULT 0,
    SubmitMac NVARCHAR(255)
);

CREATE TABLE Submission (
    SubmissionId INT IDENTITY(1,1) PRIMARY KEY,
    ProblemId INT NOT NULL,
    CoderId INT NOT NULL,
    TakePartId INT,
    SubmitTime DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    SubmitCode NVARCHAR(MAX) NOT NULL,
    SubmissionStatus NVARCHAR(50) NOT NULL CHECK (SubmissionStatus IN ('Pending', 'Accepted', 'Wrong Answer', 'Runtime Error', 'Time Limit Exceeded')),
    Score INT NOT NULL DEFAULT 0,
    ExecutionTime INT,
    ErrorMessage NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL
);

CREATE TABLE TestCase (
    TestCaseId INT IDENTITY(1,1) PRIMARY KEY,
    ProblemId INT NOT NULL,
    InitDataId INT,
    AnswerResultPath NVARCHAR(255) NOT NULL,
    IsHidden BIT NOT NULL DEFAULT 0,
    OrderNumber INT NOT NULL CHECK (OrderNumber > 0),
    Score INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL
);

CREATE TABLE TestRun (
    TestRunId INT IDENTITY(1,1) PRIMARY KEY,
    SubmissionId INT NOT NULL,
    TestCaseId INT NOT NULL,
    ActualOutput NVARCHAR(MAX),
    IsCorrect BIT NOT NULL DEFAULT 0,
    ExecutionTime INT,
    ErrorMessage NVARCHAR(MAX),
    Score INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL
);

CREATE TABLE Announcement (
    AnnouncementId INT IDENTITY(1,1) PRIMARY KEY,
    ContestId INT NOT NULL,
    AnnounceTime DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    AnnounceContent NVARCHAR(MAX) NOT NULL
);

CREATE TABLE HasProblem (
    HasProblemId INT IDENTITY(1,1) PRIMARY KEY,
    ContestId INT NOT NULL,
    ProblemId INT NOT NULL,
    ProblemOrder INT NOT NULL CHECK (ProblemOrder > 0),
    PointProblem INT NOT NULL CHECK (PointProblem >= 0)
);

CREATE TABLE Theme (
    ThemeId INT IDENTITY(1,1) PRIMARY KEY,
    ThemeName NVARCHAR(255) NOT NULL UNIQUE,
    ThemeOrder INT NOT NULL CHECK (ThemeOrder > 0)
);

CREATE TABLE ChatMessages (
    ChatMessageId INT IDENTITY(1,1) PRIMARY KEY,
    CoderId INT NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    CONSTRAINT FK_ChatMessages_Coder FOREIGN KEY (CoderId) 
        REFERENCES Coder(CoderId) ON DELETE NO ACTION
);

ALTER TABLE DatabaseSchema ADD
    CONSTRAINT FK_DatabaseSchema_Coder FOREIGN KEY (CoderId) REFERENCES Coder(CoderId) ON DELETE NO ACTION;

ALTER TABLE Problem ADD
    CONSTRAINT FK_Problem_Coder FOREIGN KEY (CoderId) REFERENCES Coder(CoderId) ON DELETE NO ACTION,
    CONSTRAINT FK_Problem_DatabaseSchema FOREIGN KEY (DatabaseSchemaId) REFERENCES DatabaseSchema(DatabaseSchemaId) ON DELETE NO ACTION,
    CONSTRAINT FK_Problem_Theme FOREIGN KEY (ThemeId) REFERENCES Theme(ThemeId) ON DELETE CASCADE;

ALTER TABLE Blog ADD
    CONSTRAINT FK_Blog_Coder FOREIGN KEY (CoderId) REFERENCES Coder(CoderId) ON DELETE NO ACTION;

ALTER TABLE Comments ADD
    CONSTRAINT FK_Comments_Blog FOREIGN KEY (BlogId) REFERENCES Blog(BlogId) ON DELETE CASCADE,
    CONSTRAINT FK_Comments_Coder FOREIGN KEY (CoderId) REFERENCES Coder(CoderId) ON DELETE NO ACTION;

ALTER TABLE Favourite ADD
    CONSTRAINT FK_Favourite_Coder FOREIGN KEY (CoderId) REFERENCES Coder(CoderId) ON DELETE NO ACTION,
    CONSTRAINT FK_Favourite_Problem FOREIGN KEY (ProblemId) REFERENCES Problem(ProblemId) ON DELETE NO ACTION;

ALTER TABLE Solved ADD
    CONSTRAINT FK_Solved_Coder FOREIGN KEY (CoderId) REFERENCES Coder(CoderId) ON DELETE NO ACTION,
    CONSTRAINT FK_Solved_Problem FOREIGN KEY (ProblemId) REFERENCES Problem(ProblemId) ON DELETE NO ACTION;

ALTER TABLE Contest ADD
    CONSTRAINT FK_Contest_Coder FOREIGN KEY (CoderId) REFERENCES Coder(CoderId) ON DELETE NO ACTION;

ALTER TABLE Participation ADD
    CONSTRAINT FK_Participation_Coder FOREIGN KEY (CoderId) REFERENCES Coder(CoderId) ON DELETE NO ACTION,
    CONSTRAINT FK_Participation_Contest FOREIGN KEY (ContestId) REFERENCES Contest(ContestId) ON DELETE CASCADE;

ALTER TABLE TakePart ADD
    CONSTRAINT FK_TakePart_Participation FOREIGN KEY (ParticipationId) REFERENCES Participation(ParticipationId) ON DELETE CASCADE,
    CONSTRAINT FK_TakePart_Problem FOREIGN KEY (ProblemId) REFERENCES Problem(ProblemId) ON DELETE NO ACTION;

ALTER TABLE Submission ADD
    CONSTRAINT FK_Submission_Problem FOREIGN KEY (ProblemId) REFERENCES Problem(ProblemId) ON DELETE NO ACTION,
    CONSTRAINT FK_Submission_Coder FOREIGN KEY (CoderId) REFERENCES Coder(CoderId) ON DELETE NO ACTION,
    CONSTRAINT FK_Submission_TakePart FOREIGN KEY (TakePartId) REFERENCES TakePart(TakePartId) ON DELETE SET NULL;

ALTER TABLE TestCase ADD
    CONSTRAINT FK_TestCase_Problem FOREIGN KEY (ProblemId) REFERENCES Problem(ProblemId) ON DELETE CASCADE,
    CONSTRAINT FK_TestCase_InitData FOREIGN KEY (InitDataId) REFERENCES InitData(InitDataId) ON DELETE SET NULL;

ALTER TABLE TestRun ADD
    CONSTRAINT FK_TestRun_Submission FOREIGN KEY (SubmissionId) REFERENCES Submission(SubmissionId) ON DELETE CASCADE,
    CONSTRAINT FK_TestRun_TestCase FOREIGN KEY (TestCaseId) REFERENCES TestCase(TestCaseId) ON DELETE NO ACTION;

ALTER TABLE Announcement ADD
    CONSTRAINT FK_Announcement_Contest FOREIGN KEY (ContestId) REFERENCES Contest(ContestId) ON DELETE CASCADE;

ALTER TABLE HasProblem ADD
    CONSTRAINT FK_HasProblem_Contest FOREIGN KEY (ContestId) REFERENCES Contest(ContestId) ON DELETE CASCADE,
    CONSTRAINT FK_HasProblem_Problem FOREIGN KEY (ProblemId) REFERENCES Problem(ProblemId) ON DELETE NO ACTION;

-- Insert sample Coders
INSERT INTO Coder (CoderName, CoderEmail, Password, AdminCoder, ContestSetter, Gender, ReceiveEmail) VALUES
('Admin User', 'admin@example.com', '774ef6a662e111656cf1455ceeb1a142', 1, 1, 1, 1),
('John Teacher', 'john@example.com', '774ef6a662e111656cf1455ceeb1a142', 0, 1, 1, 1),
('Alice Student', 'alice@example.com', '774ef6a662e111656cf1455ceeb1a142', 0, 0, 0, 1),
('Bob Student', 'bob@example.com', '774ef6a662e111656cf1455ceeb1a142', 0, 0, 1, 1);

-- Insert sample DatabaseSchemas thêm CoderId
INSERT INTO DatabaseSchema (SchemaName, SchemaDefinitionPath, Description, CoderId) VALUES
('Shop Database', '/Data/Schemas/shop_schema.csv', N'Cơ sở dữ liệu mẫu về shop bán hàng', 1),
('Library Database', '/Data/Schemas/library_schema.csv', N'Cơ sở dữ liệu về thư viện', 1),
('Employee Database', '/Data/Schemas/employee_schema.csv', N'Cơ sở dữ liệu về nhân viên và phòng ban', 1),
('School Database', '/Data/Schemas/school_schema.csv', N'Cơ sở dữ liệu về trường học và học sinh', 1),
('Hospital Database', '/Data/Schemas/hospital_schema.csv', N'Cơ sở dữ liệu về bệnh viện và bệnh nhân', 1),
('Restaurant Database', '/Data/Schemas/restaurant_schema.csv', N'Cơ sở dữ liệu về nhà hàng và món ăn', 1),
('Movie Database', '/Data/Schemas/movie_schema.csv', N'Cơ sở dữ liệu về phim và rạp chiếu', 1),
('Hotel Database', '/Data/Schemas/hotel_schema.csv', N'Cơ sở dữ liệu về khách sạn và đặt phòng', 1),
('Bank Database', '/Data/Schemas/bank_schema.csv', N'Cơ sở dữ liệu về ngân hàng và tài khoản', 1),
('University Database', '/Data/Schemas/university_schema.csv', N'Cơ sở dữ liệu về trường đại học và sinh viên', 1);

-- Insert dữ liệu vào bảng InitData
INSERT INTO InitData (DatabaseSchemaId, DataName, DataContentPath, Description) VALUES
(1, 'Shop Data 1 - Electronics and Books', '/Data/InitData/shop_data1.csv', N'Dữ liệu mẫu cơ bản cho shop bán hàng'),
(1, 'Shop Data 2 - Clothing and Food', '/Data/InitData/shop_data2.csv', N'Dữ liệu khác cho shop bán hàng'),
(2, 'Library Data 1', '/Data/InitData/library_data1.csv', N'Dữ liệu mẫu cơ bản cho thư viện'),
(2, 'Library Data 2 - Multiple Books', '/Data/InitData/library_data2.csv', N'Dữ liệu mẫu phong phú hơn cho thư viện'),
(3, 'Employee Data 1', '/Data/InitData/employee_data1.csv', N'Dữ liệu mẫu về nhân viên văn phòng'),
(3, 'Employee Data 2', '/Data/InitData/employee_data2.csv', N'Dữ liệu mẫu về nhân viên kỹ thuật'),
(4, 'School Data 1', '/Data/InitData/school_data1.csv', N'Dữ liệu mẫu về học sinh cấp 1'),
(4, 'School Data 2', '/Data/InitData/school_data2.csv', N'Dữ liệu mẫu về học sinh cấp 2'),
(5, 'Hospital Data 1', '/Data/InitData/hospital_data1.csv', N'Dữ liệu mẫu về bệnh nhân nội trú'),
(5, 'Hospital Data 2', '/Data/InitData/hospital_data2.csv', N'Dữ liệu mẫu về bệnh nhân ngoại trú'),
(6, 'Restaurant Data 1', '/Data/InitData/restaurant_data1.csv', N'Dữ liệu mẫu về món ăn Á'),
(6, 'Restaurant Data 2', '/Data/InitData/restaurant_data2.csv', N'Dữ liệu mẫu về món ăn Âu'),
(7, 'Movie Data 1', '/Data/InitData/movie_data1.csv', N'Dữ liệu mẫu về phim đang chiếu'),
(7, 'Movie Data 2', '/Data/InitData/movie_data2.csv', N'Dữ liệu mẫu về phim sắp chiếu'),
(8, 'Hotel Data 1', '/Data/InitData/hotel_data1.csv', N'Dữ liệu mẫu về phòng đơn'),
(8, 'Hotel Data 2', '/Data/InitData/hotel_data2.csv', N'Dữ liệu mẫu về phòng đôi'),
(9, 'Bank Data 1', '/Data/InitData/bank_data1.csv', N'Dữ liệu mẫu về tài khoản tiết kiệm'),
(9, 'Bank Data 2', '/Data/InitData/bank_data2.csv', N'Dữ liệu mẫu về tài khoản vay'),
(10, 'University Data 1', '/Data/InitData/university_data1.csv', N'Dữ liệu mẫu về sinh viên năm nhất'),
(10, 'University Data 2', '/Data/InitData/university_data2.csv', N'Dữ liệu mẫu về sinh viên năm cuối');

-- Insert sample Themes
INSERT INTO Theme (ThemeName, ThemeOrder) VALUES
(N'SELECT cơ bản', 1),
(N'WHERE và ORDER BY', 2),
(N'GROUP BY và HAVING', 3),
(N'JOIN các bảng', 4),
(N'Subquery', 5),
(N'INSERT, UPDATE, DELETE', 6);

-- Insert sample Problems với ThemeId và thêm AnswerQuery
INSERT INTO Problem (ProblemCode, ProblemName, ProblemDescription, DatabaseSchemaId, CoderId, Published, ThemeId, AnswerQueryPath) VALUES
('P001', N'Tìm sản phẩm đắt nhất', N'Viết câu lệnh SQL để tìm sản phẩm có giá cao nhất trong bảng Products', 1, 1, 1, 1, '/Data/AnswerQuerys/p001_answer_query.csv'),
('P002', N'Đếm số sách theo tác giả', N'Viết câu lệnh SQL để đếm số lượng sách của mỗi tác giả', 2, 1, 1, 3, '/Data/AnswerQuerys/p002_answer_query.csv'),
('P003', N'Tìm nhân viên có lương cao nhất', N'Viết câu lệnh SQL để tìm nhân viên có mức lương cao nhất trong công ty', 3, 1, 1, 1, '/Data/AnswerQuerys/p003_answer_query.csv'),
('P004', N'Thống kê học sinh theo lớp', N'Viết câu lệnh SQL để đếm số học sinh trong mỗi lớp', 4, 1, 1, 3, '/Data/AnswerQuerys/p004_answer_query.csv'),
('P005', N'Danh sách bệnh nhân theo khoa', N'Viết câu lệnh SQL để liệt kê bệnh nhân theo từng khoa', 5, 1, 1, 4, '/Data/AnswerQuerys/p005_answer_query.csv'),
('P006', N'Thống kê doanh thu món ăn', N'Viết câu lệnh SQL để tính tổng doanh thu của từng món ăn', 6, 1, 1, 3, '/Data/AnswerQuerys/p006_answer_query.csv'),
('P007', N'Tìm phim được đặt vé nhiều nhất', N'Viết câu lệnh SQL để tìm phim có số lượng vé đặt cao nhất', 7, 1, 1, 5, '/Data/AnswerQuerys/p007_answer_query.csv'),
('P008', N'Thống kê phòng trống theo ngày', N'Viết câu lệnh SQL để đếm số phòng trống theo từng ngày', 8, 1, 1, 3, '/Data/AnswerQuerys/p008_answer_query.csv'),
('P009', N'Tìm khách hàng có số dư lớn nhất', N'Viết câu lệnh SQL để tìm khách hàng có số dư tài khoản lớn nhất', 9, 1, 1, 1, '/Data/AnswerQuerys/p009_answer_query.csv'),
('P010', N'Thống kê sinh viên theo ngành', N'Viết câu lệnh SQL để đếm số sinh viên theo từng ngành học', 10, 1, 1, 3, '/Data/AnswerQuerys/p010_answer_query.csv'),
('P011', N'Tìm sách mượn nhiều nhất', N'Viết câu lệnh SQL để tìm những cuốn sách được mượn nhiều nhất trong thư viện', 2, 1, 1, 1, '/Data/AnswerQuerys/p011_answer_query.csv'),
('P012', N'Thống kê doanh thu theo nhân viên', N'Viết câu lệnh SQL để tính tổng doanh thu bán hàng của từng nhân viên, sắp xếp theo doanh thu giảm dần', 1, 1, 1, 3, '/Data/AnswerQuerys/p012_answer_query.csv'),
('P013', N'Phân tích lương theo phòng ban', N'Viết câu lệnh SQL để phân tích mức lương trung bình, cao nhất, thấp nhất của từng phòng ban', 3, 1, 1, 3, '/Data/AnswerQuerys/p013_answer_query.csv'),
('P014', N'Tìm học sinh giỏi nhất môn', N'Viết câu lệnh SQL để tìm học sinh có điểm cao nhất trong từng môn học', 4, 1, 1, 4, '/Data/AnswerQuerys/p014_answer_query.csv'),
('P015', N'Thống kê bệnh nhân theo bác sĩ', N'Viết câu lệnh SQL để đếm số lượng bệnh nhân đang điều trị của từng bác sĩ', 5, 1, 1, 3, '/Data/AnswerQuerys/p015_answer_query.csv'),
('P016', N'Phân tích đơn giá món ăn', N'Viết câu lệnh SQL để phân tích giá trung bình, cao nhất, thấp nhất của món ăn theo từng danh mục', 6, 1, 1, 3, '/Data/AnswerQuerys/p016_answer_query.csv'),
('P017', N'Doanh thu phim theo thể loại', N'Viết câu lệnh SQL để tính tổng doanh thu bán vé của từng thể loại phim', 7, 1, 1, 3, '/Data/AnswerQuerys/p017_answer_query.csv'),
('P018', N'Tỷ lệ đặt phòng theo loại', N'Viết câu lệnh SQL để tính tỷ lệ đặt phòng (phòng đã đặt/tổng số phòng) của từng loại phòng', 8, 1, 1, 5, '/Data/AnswerQuerys/p018_answer_query.csv'),
('P019', N'Phân tích giao dịch khách hàng', N'Viết câu lệnh SQL để phân tích số lượng và giá trị trung bình các giao dịch của từng khách hàng', 9, 1, 1, 3, '/Data/AnswerQuerys/p019_answer_query.csv'),
('P020', N'Điểm trung bình theo khoa', N'Viết câu lệnh SQL để tính điểm trung bình của sinh viên theo từng khoa và xếp hạng các khoa', 10, 1, 1, 3, '/Data/AnswerQuerys/p020_answer_query.csv');

-- Insert sample TestCases với InitDataId
INSERT INTO TestCase (ProblemId, InitDataId, AnswerResultPath, IsHidden, OrderNumber, Score) VALUES
(1, 1, '/Data/AnswerResults/p001_testcase1.csv', 0, 1, 50),
(1, 2, '/Data/AnswerResults/p001_testcase2.csv', 0, 2, 50),
(2, 3, '/Data/AnswerResults/p002_testcase1.csv', 0, 1, 50),
(2, 4, '/Data/AnswerResults/p002_testcase2.csv', 0, 2, 50),
(3, 5, '/Data/AnswerResults/p003_testcase1.csv', 0, 1, 50),
(3, 6, '/Data/AnswerResults/p003_testcase2.csv', 0, 2, 50),
(4, 7, '/Data/AnswerResults/p004_testcase1.csv', 0, 1, 50),
(4, 8, '/Data/AnswerResults/p004_testcase2.csv', 0, 2, 50),
(5, 9, '/Data/AnswerResults/p005_testcase1.csv', 0, 1, 50),
(5, 10, '/Data/AnswerResults/p005_testcase2.csv', 0, 2, 50),
(6, 11, '/Data/AnswerResults/p006_testcase1.csv', 0, 1, 50),
(6, 12, '/Data/AnswerResults/p006_testcase2.csv', 0, 2, 50),
(7, 13, '/Data/AnswerResults/p007_testcase1.csv', 0, 1, 50),
(7, 14, '/Data/AnswerResults/p007_testcase2.csv', 0, 2, 50),
(8, 15, '/Data/AnswerResults/p008_testcase1.csv', 0, 1, 50),
(8, 16, '/Data/AnswerResults/p008_testcase2.csv', 0, 2, 50),
(9, 17, '/Data/AnswerResults/p009_testcase1.csv', 0, 1, 50),
(9, 18, '/Data/AnswerResults/p009_testcase2.csv', 0, 2, 50),
(10, 19, '/Data/AnswerResults/p010_testcase1.csv', 0, 1, 50),
(10, 20, '/Data/AnswerResults/p010_testcase2.csv', 0, 2, 50),
(11, 3, '/Data/AnswerResults/p011_testcase1.csv', 0, 1, 40),
(11, 3, '/Data/AnswerResults/p011_testcase2.csv', 0, 2, 30),
(11, 4, '/Data/AnswerResults/p011_testcase3.csv', 0, 3, 30),
(12, 1, '/Data/AnswerResults/p012_testcase1.csv', 0, 1, 50),
(12, 2, '/Data/AnswerResults/p012_testcase2.csv', 0, 2, 50),
(13, 5, '/Data/AnswerResults/p013_testcase1.csv', 0, 1, 30),
(13, 5, '/Data/AnswerResults/p013_testcase2.csv', 0, 2, 35),
(13, 6, '/Data/AnswerResults/p013_testcase3.csv', 0, 3, 35),
(14, 7, '/Data/AnswerResults/p014_testcase1.csv', 0, 1, 50),
(14, 8, '/Data/AnswerResults/p014_testcase2.csv', 0, 2, 50),
(15, 9, '/Data/AnswerResults/p015_testcase1.csv', 0, 1, 50),
(15, 10, '/Data/AnswerResults/p015_testcase2.csv', 0, 2, 50),
(16, 11, '/Data/AnswerResults/p016_testcase1.csv', 0, 1, 30),
(16, 11, '/Data/AnswerResults/p016_testcase2.csv', 0, 2, 35),
(16, 12, '/Data/AnswerResults/p016_testcase3.csv', 0, 3, 35),
(17, 13, '/Data/AnswerResults/p017_testcase1.csv', 0, 1, 50),
(17, 14, '/Data/AnswerResults/p017_testcase2.csv', 0, 2, 50),
(18, 15, '/Data/AnswerResults/p018_testcase1.csv', 0, 1, 50),
(18, 16, '/Data/AnswerResults/p018_testcase2.csv', 0, 2, 50),
(19, 17, '/Data/AnswerResults/p019_testcase1.csv', 0, 1, 30),
(19, 17, '/Data/AnswerResults/p019_testcase2.csv', 0, 2, 35),
(19, 18, '/Data/AnswerResults/p019_testcase3.csv', 0, 3, 35),
(20, 19, '/Data/AnswerResults/p020_testcase1.csv', 0, 1, 50),
(20, 20, '/Data/AnswerResults/p020_testcase2.csv', 0, 2, 50); 

-- Insert sample Contest
INSERT INTO Contest (
    ContestName, 
    ContestDescription, 
    StartTime, 
    EndTime, 
    Duration, 
    CoderId, 
    StatusContest, 
    Published,
    RuleType,
    FailedPenalty
) VALUES (
    N'SQL Cơ bản', 
    N'Contest về các câu lệnh SQL cơ bản', 
    DATEADD(DAY, 1, GETDATE()), 
    DATEADD(DAY, 2, GETDATE()), 
    120, 
    1, 
    'Pending', 
    1,
    'IOI',
    0
);

-- Đợi Contest được tạo xong mới insert HasProblem
DECLARE @ContestId INT = SCOPE_IDENTITY();

-- Insert HasProblem với ContestId vừa tạo
INSERT INTO HasProblem (ContestId, ProblemId, ProblemOrder, PointProblem) VALUES
(@ContestId, 1, 1, 100),
(@ContestId, 2, 2, 100);

-- Insert sample Blog
INSERT INTO Blog (BlogTitle, BlogContent, CoderId, Published, PinHome) VALUES
(N'Giới thiệu về SQL', N'SQL (Structured Query Language) là ngôn ngữ truy vấn có cấu trúc...', 1, 1, 1),
(N'Tips học SQL hiệu quả', N'Để học SQL hiệu quả, bạn cần...', 1, 1, 0);

-- Insert sample Comments
INSERT INTO Comments (BlogId, CoderId, CommentContent) VALUES
(1, 2, N'Bài viết rất hữu ích!'),
(1, 3, N'Cảm ơn admin đã chia sẻ');

-- Insert sample ChatMessages
INSERT INTO ChatMessages (CoderId, Message) VALUES
(1, N'Xin chào mọi người!'),
(2, N'Chào admin, tôi cần hỗ trợ về bài tập SQL'),
(1, N'Vâng, tôi có thể giúp gì cho bạn?');
