USE master;
GO
DROP DATABASE IF EXISTS sql_exercise_scoring;
GO
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

CREATE TABLE DatabaseSchema (
    DatabaseSchemaId INT IDENTITY(1,1) PRIMARY KEY,
    SchemaName NVARCHAR(255) NOT NULL UNIQUE,
    SchemaDefinition NVARCHAR(MAX) NOT NULL,
    InitialData NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL
);

CREATE TABLE Problem (
    ProblemId INT IDENTITY(1,1) PRIMARY KEY,
    ProblemCode NVARCHAR(255) NOT NULL UNIQUE,
    ProblemName NVARCHAR(255) NOT NULL,
    ProblemDescription NVARCHAR(MAX) NOT NULL,
    ProblemExplanation NVARCHAR(MAX),
    Published BIT NOT NULL DEFAULT 0,
    CoderId INT NOT NULL,
    DatabaseSchemaId INT NOT NULL,
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
    TestQuery NVARCHAR(MAX) NOT NULL,
    ExpectedResult NVARCHAR(MAX) NOT NULL,
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

CREATE TABLE ProblemTheme (
    ProblemId INT NOT NULL,
    ThemeId INT NOT NULL,
    Note NVARCHAR(MAX),
    PRIMARY KEY (ProblemId, ThemeId)
);

-- Tạo bảng ChatMessages
CREATE TABLE ChatMessages (
    ChatMessageId INT IDENTITY(1,1) PRIMARY KEY,
    CoderId INT NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    CONSTRAINT FK_ChatMessages_Coder FOREIGN KEY (CoderId) 
        REFERENCES Coder(CoderId) ON DELETE NO ACTION
);

-- Add all foreign keys with proper ON DELETE/UPDATE actions
-- ... existing code (giữ nguyên phần CREATE TABLE) ...

-- Sửa lại tất cả các ràng buộc FOREIGN KEY
ALTER TABLE Problem ADD
    CONSTRAINT FK_Problem_Coder FOREIGN KEY (CoderId) REFERENCES Coder(CoderId) ON DELETE NO ACTION,
    CONSTRAINT FK_Problem_DatabaseSchema FOREIGN KEY (DatabaseSchemaId) REFERENCES DatabaseSchema(DatabaseSchemaId) ON DELETE NO ACTION;

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
    CONSTRAINT FK_TestCase_Problem FOREIGN KEY (ProblemId) REFERENCES Problem(ProblemId) ON DELETE CASCADE;

ALTER TABLE TestRun ADD
    CONSTRAINT FK_TestRun_Submission FOREIGN KEY (SubmissionId) REFERENCES Submission(SubmissionId) ON DELETE CASCADE,
    CONSTRAINT FK_TestRun_TestCase FOREIGN KEY (TestCaseId) REFERENCES TestCase(TestCaseId) ON DELETE NO ACTION;

ALTER TABLE Announcement ADD
    CONSTRAINT FK_Announcement_Contest FOREIGN KEY (ContestId) REFERENCES Contest(ContestId) ON DELETE CASCADE;

ALTER TABLE HasProblem ADD
    CONSTRAINT FK_HasProblem_Contest FOREIGN KEY (ContestId) REFERENCES Contest(ContestId) ON DELETE CASCADE,
    CONSTRAINT FK_HasProblem_Problem FOREIGN KEY (ProblemId) REFERENCES Problem(ProblemId) ON DELETE NO ACTION;

ALTER TABLE ProblemTheme ADD
    CONSTRAINT FK_ProblemTheme_Problem FOREIGN KEY (ProblemId) REFERENCES Problem(ProblemId) ON DELETE NO ACTION,
    CONSTRAINT FK_ProblemTheme_Theme FOREIGN KEY (ThemeId) REFERENCES Theme(ThemeId) ON DELETE NO ACTION;


-- Insert sample Coders
INSERT INTO Coder (CoderName, CoderEmail, Password, AdminCoder, ContestSetter, Gender, ReceiveEmail) VALUES
('Admin User', 'admin@example.com', 'hashed_password_123', 1, 1, 1, 1),
('John Teacher', 'john@example.com', 'hashed_password_456', 0, 1, 1, 1),
('Alice Student', 'alice@example.com', 'hashed_password_789', 0, 0, 0, 1),
('Bob Student', 'bob@example.com', 'hashed_password_101', 0, 0, 1, 1);

-- Insert sample DatabaseSchemas
INSERT INTO DatabaseSchema (SchemaName, SchemaDefinition, InitialData, Description) VALUES
('Shop Database', 
'CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY,
    CategoryName NVARCHAR(100)
);
CREATE TABLE Products (
    ProductID INT PRIMARY KEY,
    ProductName NVARCHAR(100),
    CategoryID INT,
    Price DECIMAL(10,2)
);',
N'INSERT INTO Categories VALUES (1, ''Electronics''), (2, ''Books'');
INSERT INTO Products VALUES 
(1, ''Laptop'', 1, 1200),
(2, ''SQL Guide'', 2, 30),
(3, ''Smartphone'', 1, 800);',
'Cơ sở dữ liệu mẫu về shop bán hàng'),

('Library Database',
'CREATE TABLE Authors (
    AuthorID INT PRIMARY KEY,
    AuthorName NVARCHAR(100)
);
CREATE TABLE Books (
    BookID INT PRIMARY KEY,
    Title NVARCHAR(200),
    AuthorID INT,
    PublishYear INT
);',
N'INSERT INTO Authors VALUES (1, ''John Smith''), (2, ''Jane Doe'');
INSERT INTO Books VALUES 
(1, ''Database Design'', 1, 2020),
(2, ''SQL Mastery'', 2, 2021);',
'Cơ sở dữ liệu về thư viện');

-- Insert sample Themes
INSERT INTO Theme (ThemeName, ThemeOrder) VALUES
(N'SELECT cơ bản', 1),
(N'WHERE và ORDER BY', 2),
(N'GROUP BY và HAVING', 3),
(N'JOIN các bảng', 4),
(N'Subquery', 5),
(N'INSERT, UPDATE, DELETE', 6);

-- Insert sample Problems (sử dụng CoderId từ bảng Coder đã insert)
INSERT INTO Problem (ProblemCode, ProblemName, ProblemDescription, DatabaseSchemaId, CoderId, Published) VALUES
('P001', N'Tìm sản phẩm đắt nhất', N'Viết câu lệnh SQL để tìm sản phẩm có giá cao nhất trong bảng Products', 1, 1, 1),
('P002', N'Đếm số sách theo tác giả', N'Viết câu lệnh SQL để đếm số lượng sách của mỗi tác giả', 2, 1, 1);

-- Insert sample TestCases
INSERT INTO TestCase (ProblemId, TestQuery, ExpectedResult, IsHidden, OrderNumber, Score) VALUES
(1, 'SELECT TOP 1 ProductName, Price FROM Products ORDER BY Price DESC', 'Laptop|1200', 0, 1, 50),
(1, 'SELECT ProductName, Price FROM Products WHERE Price = (SELECT MAX(Price) FROM Products)', 'Laptop|1200', 0, 2, 50),
(2, 'SELECT a.AuthorName, COUNT(b.BookID) as BookCount FROM Authors a LEFT JOIN Books b ON a.AuthorID = b.AuthorID GROUP BY a.AuthorName', 'John Smith|1\nJane Doe|1', 0, 1, 100);

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
    RuleType,      -- Thêm trường RuleType
    FailedPenalty  -- Thêm trường FailedPenalty
) VALUES (
    N'SQL Cơ bản', 
    N'Contest về các câu lệnh SQL cơ bản', 
    DATEADD(DAY, 1, GETDATE()), 
    DATEADD(DAY, 2, GETDATE()), 
    120, 
    1, 
    'Pending', 
    1,
    'IOI',         -- Giá trị cho RuleType (ACM, IOI, hoặc Custom)
    0              -- Giá trị cho FailedPenalty
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

INSERT INTO ProblemTheme (ProblemId, ThemeId) VALUES
(1, 1),
(2, 2);

-- Insert sample ChatMessages
INSERT INTO ChatMessages (CoderId, Message) VALUES
(1, N'Xin chào mọi người!'),
(2, N'Chào admin, tôi cần hỗ trợ về bài tập SQL'),
(1, N'Vâng, tôi có thể giúp gì cho bạn?');
