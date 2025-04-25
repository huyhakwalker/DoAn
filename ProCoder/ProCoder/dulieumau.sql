--Schema: 1
--E-Commerce Database, 
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Phone NVARCHAR(20),
    Address NVARCHAR(200),
    City NVARCHAR(50),
    Country NVARCHAR(50),
    RegistrationDate DATETIME DEFAULT GETDATE()
);
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY,
    CategoryName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200)
);
CREATE TABLE Products (
    ProductID INT PRIMARY KEY,
    ProductName NVARCHAR(100) NOT NULL,
    CategoryID INT FOREIGN KEY REFERENCES Categories(CategoryID),
    Price DECIMAL(10,2) NOT NULL,
    Description NVARCHAR(MAX),
    StockQuantity INT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE()
);
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY,
    CustomerID INT FOREIGN KEY REFERENCES Customers(CustomerID),
    OrderDate DATETIME DEFAULT GETDATE(),
    ShippingAddress NVARCHAR(200),
    TotalAmount DECIMAL(12,2),
    Status NVARCHAR(20) DEFAULT ''Pending''
);
CREATE TABLE OrderDetails (
    OrderDetailID INT PRIMARY KEY,
    OrderID INT FOREIGN KEY REFERENCES Orders(OrderID),
    ProductID INT FOREIGN KEY REFERENCES Products(ProductID),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL
);
--'Schema mẫu cho hệ thống bán hàng trực tuyến với các bảng Customers, Categories, Products, Orders và OrderDetails.',


--DATA: 1
--E-Commerce Sample Data 1
INSERT INTO Customers (CustomerID, FirstName, LastName, Email, Phone, Address, City, Country)
VALUES 
(1, ''Nguyễn'', ''Văn An'', ''nguyenvanan@email.com'', ''0912345678'', ''123 Lê Lợi'', ''Hà Nội'', ''Việt Nam''),
(2, ''Trần'', ''Thị Bình'', ''tranthibinh@email.com'', ''0923456789'', ''456 Nguyễn Huệ'', ''Hồ Chí Minh'', ''Việt Nam''),
(3, ''Lê'', ''Hoàng'', ''lehoang@email.com'', ''0934567890'', ''789 Trần Phú'', ''Đà Nẵng'', ''Việt Nam'');

-- Dữ liệu cho bảng Categories
INSERT INTO Categories (CategoryID, CategoryName, Description)
VALUES 
(1, ''Điện thoại'', ''Các sản phẩm điện thoại di động''),
(2, ''Laptop'', ''Các sản phẩm máy tính xách tay''),
(3, ''Phụ kiện'', ''Phụ kiện điện tử các loại'');

-- Dữ liệu cho bảng Products
INSERT INTO Products (ProductID, ProductName, CategoryID, Price, Description, StockQuantity)
VALUES 
(1, ''Smartphone XYZ'', 1, 5000000, ''Điện thoại thông minh cao cấp'', 50),
(2, ''Laptop ABC'', 2, 15000000, ''Laptop gaming hiệu năng cao'', 30),
(3, ''Tai nghe không dây'', 3, 1200000, ''Tai nghe bluetooth chống ồn'', 100),
(4, ''Smartphone Premium'', 1, 10000000, ''Điện thoại cao cấp nhất thị trường'', 25),
(5, ''Laptop Ultrabook'', 2, 20000000, ''Laptop mỏng nhẹ cấu hình mạnh'', 15);

-- Dữ liệu cho bảng Orders
INSERT INTO Orders (OrderID, CustomerID, OrderDate, ShippingAddress, TotalAmount, Status)
VALUES 
(1, 1, ''2023-01-15'', ''123 Lê Lợi, Hà Nội'', 5000000, ''Delivered''),
(2, 2, ''2023-02-20'', ''456 Nguyễn Huệ, Hồ Chí Minh'', 16200000, ''Processing''),
(3, 3, ''2023-03-10'', ''789 Trần Phú, Đà Nẵng'', 20000000, ''Shipped'');

-- Dữ liệu cho bảng OrderDetails
INSERT INTO OrderDetails (OrderDetailID, OrderID, ProductID, Quantity, UnitPrice)
VALUES 
(1, 1, 1, 1, 5000000),
(2, 2, 2, 1, 15000000),
(3, 2, 3, 1, 1200000),
(4, 3, 5, 1, 20000000);
--Bộ dữ liệu mẫu cho hệ thống bán hàng với các sản phẩm điện tử và đơn hàng mẫu


--DATA: 2
--E-Commerce Electronics Data
INSERT INTO Customers (CustomerID, FirstName, LastName, Email, Phone, Address, City, Country)
VALUES 
(1, ''Hoàng'', ''Minh'', ''hoangminh@email.com'', ''0912345678'', ''15 Lý Thường Kiệt'', ''Hà Nội'', ''Việt Nam''),
(2, ''Phạm'', ''Hương'', ''phamhuong@email.com'', ''0923456789'', ''28 Lê Duẩn'', ''Hồ Chí Minh'', ''Việt Nam''),
(3, ''Vũ'', ''Anh'', ''vuanh@email.com'', ''0934567890'', ''47 Nguyễn Chí Thanh'', ''Đà Nẵng'', ''Việt Nam''),
(4, ''Đỗ'', ''Linh'', ''dolinh@email.com'', ''0945678901'', ''55 Trần Duy Hưng'', ''Hà Nội'', ''Việt Nam''),
(5, ''Ngô'', ''Quân'', ''ngoquan@email.com'', ''0956789012'', ''123 Võ Văn Tần'', ''Hồ Chí Minh'', ''Việt Nam'');
INSERT INTO Categories (CategoryID, CategoryName, Description)
VALUES 
(1, ''Smartphone'', ''Điện thoại thông minh các loại''),
(2, ''Laptop'', ''Máy tính xách tay''),
(3, ''Tablet'', ''Máy tính bảng''),
(4, ''Smartwatch'', ''Đồng hồ thông minh''),
(5, ''Headphones'', ''Tai nghe các loại'');
INSERT INTO Products (ProductID, ProductName, CategoryID, Price, Description, StockQuantity)
VALUES 
(1, ''iPhone 13'', 1, 21990000, ''iPhone 13 128GB chính hãng'', 50),
(2, ''Samsung Galaxy S21'', 1, 18990000, ''Samsung Galaxy S21 Ultra 5G'', 40),
(3, ''MacBook Pro M1'', 2, 31990000, ''MacBook Pro M1 13 inch 2020'', 25),
(4, ''Dell XPS 13'', 2, 27990000, ''Dell XPS 13 9310 i7'', 20),
(5, ''iPad Pro'', 3, 19990000, ''iPad Pro 11 inch M1 2021'', 35),
(6, ''Apple Watch Series 7'', 4, 10990000, ''Apple Watch Series 7 GPS'', 45),
(7, ''AirPods Pro'', 5, 5990000, ''Apple AirPods Pro'', 60),
(8, ''Sony WH-1000XM4'', 5, 7490000, ''Sony WH-1000XM4 Noise Canceling'', 30);
INSERT INTO Orders (OrderID, CustomerID, OrderDate, ShippingAddress, TotalAmount, Status)
VALUES 
(1, 1, ''2023-03-10'', ''15 Lý Thường Kiệt, Hà Nội'', 21990000, ''Delivered''),
(2, 2, ''2023-03-15'', ''28 Lê Duẩn, Hồ Chí Minh'', 49980000, ''Processing''),
(3, 3, ''2023-03-20'', ''47 Nguyễn Chí Thanh, Đà Nẵng'', 18990000, ''Shipped''),
(4, 4, ''2023-03-25'', ''55 Trần Duy Hưng, Hà Nội'', 37980000, ''Pending''),
(5, 5, ''2023-03-30'', ''123 Võ Văn Tần, Hồ Chí Minh'', 31990000, ''Processing'');
INSERT INTO OrderDetails (OrderDetailID, OrderID, ProductID, Quantity, UnitPrice)
VALUES 
(1, 1, 1, 1, 21990000),
(2, 2, 3, 1, 31990000),
(3, 2, 7, 3, 5990000),
(4, 3, 2, 1, 18990000),
(5, 4, 5, 1, 19990000),
(6, 4, 6, 1, 10990000),
(7, 4, 7, 1, 5990000),
(8, 5, 3, 1, 31990000);
--Bộ dữ liệu mẫu chi tiết cho cửa hàng bán thiết bị điện tử cao cấp











--Schema: 2
--Hospital Management System
CREATE TABLE Departments (
    DepartmentID INT PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL,
    Location NVARCHAR(100),
    HeadOfDepartment NVARCHAR(100)
);
CREATE TABLE Doctors (
    DoctorID INT PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Specialization NVARCHAR(100),
    DepartmentID INT FOREIGN KEY REFERENCES Departments(DepartmentID),
    ContactNumber NVARCHAR(20),
    Email NVARCHAR(100),
    JoiningDate DATE DEFAULT GETDATE()
);
CREATE TABLE Patients (
    PatientID INT PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Gender NVARCHAR(10),
    DateOfBirth DATE,
    BloodGroup NVARCHAR(5),
    ContactNumber NVARCHAR(20),
    Address NVARCHAR(200),
    RegistrationDate DATE DEFAULT GETDATE()
);
CREATE TABLE Appointments (
    AppointmentID INT PRIMARY KEY,
    PatientID INT FOREIGN KEY REFERENCES Patients(PatientID),
    DoctorID INT FOREIGN KEY REFERENCES Doctors(DoctorID),
    AppointmentDate DATETIME,
    Description NVARCHAR(MAX),
    Status NVARCHAR(20) DEFAULT ''Scheduled''
);
CREATE TABLE Medications (
    MedicationID INT PRIMARY KEY,
    MedicationName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(200),
    UnitPrice DECIMAL(10,2)
);
CREATE TABLE Prescriptions (
    PrescriptionID INT PRIMARY KEY,
    AppointmentID INT FOREIGN KEY REFERENCES Appointments(AppointmentID),
    MedicationID INT FOREIGN KEY REFERENCES Medications(MedicationID),
    Dosage NVARCHAR(50),
    Frequency NVARCHAR(50),
    StartDate DATE,
    EndDate DATE
);
--'Schema mẫu cho hệ thống quản lý bệnh viện với các bảng Departments, Doctors, Patients, Appointments, Medications và Prescriptions.',



--Data: 1
--Hospital Sample Data 1
INSERT INTO Departments (DepartmentID, DepartmentName, Location, HeadOfDepartment)
VALUES 
(1, ''Khoa Nội'', ''Tầng 2, Tòa nhà A'', ''PGS.TS. Nguyễn Văn A''),
(2, ''Khoa Ngoại'', ''Tầng 3, Tòa nhà A'', ''PGS.TS. Trần Thị B''),
(3, ''Khoa Nhi'', ''Tầng 1, Tòa nhà B'', ''GS.TS. Lê Văn C''),
(4, ''Khoa Sản'', ''Tầng 2, Tòa nhà B'', ''TS. Phạm Thị D''),
(5, ''Khoa Tim mạch'', ''Tầng 4, Tòa nhà A'', ''GS.TS. Hoàng Văn E'');
INSERT INTO Doctors (DoctorID, FirstName, LastName, Specialization, DepartmentID, ContactNumber, Email, JoiningDate)
VALUES 
(1, ''Nguyễn'', ''Văn Nam'', ''Nội tổng quát'', 1, ''0912345678'', ''nguyenvannam@hospital.com'', ''2015-05-10''),
(2, ''Trần'', ''Thị Hoa'', ''Ngoại tổng quát'', 2, ''0923456789'', ''tranthihoa@hospital.com'', ''2016-03-15''),
(3, ''Lê'', ''Minh'', ''Nhi khoa'', 3, ''0934567890'', ''leminh@hospital.com'', ''2017-07-20''),
(4, ''Phạm'', ''Thị Lan'', ''Sản phụ khoa'', 4, ''0945678901'', ''phamthilan@hospital.com'', ''2014-09-25''),
(5, ''Hoàng'', ''Anh'', ''Tim mạch'', 5, ''0956789012'', ''hoanganh@hospital.com'', ''2018-11-30'');
INSERT INTO Patients (PatientID, FirstName, LastName, Gender, DateOfBirth, BloodGroup, ContactNumber, Address, RegistrationDate)
VALUES 
(1, ''Trương'', ''Văn Bình'', ''Nam'', ''1980-04-15'', ''A+'', ''0912345678'', ''123 Nguyễn Trãi, Quận 1, TP.HCM'', ''2023-01-10''),
(2, ''Ngô'', ''Thị Hồng'', ''Nữ'', ''1975-08-20'', ''B-'', ''0923456789'', ''456 Lê Lợi, Quận 5, TP.HCM'', ''2023-01-15''),
(3, ''Đặng'', ''Minh Tuấn'', ''Nam'', ''1990-12-05'', ''O+'', ''0934567890'', ''789 Trần Hưng Đạo, Quận 3, TP.HCM'', ''2023-01-20''),
(4, ''Vũ'', ''Thị Mai'', ''Nữ'', ''1985-03-25'', ''AB+'', ''0945678901'', ''101 Nguyễn Đình Chiểu, Quận 3, TP.HCM'', ''2023-01-25''),
(5, ''Lý'', ''Văn Dũng'', ''Nam'', ''1995-07-30'', ''A-'', ''0956789012'', ''202 Điện Biên Phủ, Quận Bình Thạnh, TP.HCM'', ''2023-01-30'');
INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, AppointmentDate, Description, Status)
VALUES 
(1, 1, 1, ''2023-03-01 09:00:00'', ''Khám tổng quát'', ''Completed''),
(2, 2, 4, ''2023-03-02 10:30:00'', ''Khám thai định kỳ'', ''Completed''),
(3, 3, 3, ''2023-03-03 14:00:00'', ''Kiểm tra sức khỏe'', ''Scheduled''),
(4, 4, 5, ''2023-03-04 15:30:00'', ''Đau ngực'', ''Cancelled''),
(5, 5, 2, ''2023-03-05 08:00:00'', ''Tham vấn phẫu thuật'', ''Scheduled'');
INSERT INTO Medications (MedicationID, MedicationName, Description, UnitPrice)
VALUES 
(1, ''Paracetamol'', ''Thuốc giảm đau, hạ sốt'', 5000),
(2, ''Amoxicillin'', ''Kháng sinh'', 15000),
(3, ''Omeprazole'', ''Điều trị dạ dày'', 20000),
(4, ''Loratadine'', ''Thuốc kháng histamin'', 25000),
(5, ''Metformin'', ''Điều trị tiểu đường'', 30000);
INSERT INTO Prescriptions (PrescriptionID, AppointmentID, MedicationID, Dosage, Frequency, StartDate, EndDate)
VALUES 
(1, 1, 1, ''500mg'', ''3 lần/ngày'', ''2023-03-01'', ''2023-03-07''),
(2, 1, 3, ''20mg'', ''1 lần/ngày'', ''2023-03-01'', ''2023-03-14''),
(3, 2, 4, ''10mg'', ''1 lần/ngày'', ''2023-03-02'', ''2023-03-09''),
(4, 3, 2, ''250mg'', ''2 lần/ngày'', ''2023-03-03'', ''2023-03-10''),
(5, 5, 5, ''500mg'', ''2 lần/ngày'', ''2023-03-05'', ''2023-03-19'');
--Bộ dữ liệu mẫu cho hệ thống quản lý bệnh viện với thông tin bệnh nhân, bác sĩ và lịch khám

--data: 2
-- Dữ liệu cho bảng Departments
INSERT INTO Departments (DepartmentID, DepartmentName, Location, HeadOfDepartment)
VALUES 
(1, ''Khoa Tim mạch'', ''Tầng 3, Tòa nhà Chuyên khoa'', ''GS.TS. Nguyễn Lân Việt''),
(2, ''Khoa Nội tiết'', ''Tầng 2, Tòa nhà Chuyên khoa'', ''PGS.TS. Trần Hữu Dàng''),
(3, ''Khoa Hồi sức tích cực'', ''Tầng 1, Tòa nhà Cấp cứu'', ''TS.BS. Lê Thành Trung''),
(4, ''Khoa Chẩn đoán hình ảnh'', ''Tầng 1, Tòa nhà Kỹ thuật'', ''TS.BS. Phạm Minh Thông''),
(5, ''Khoa Xét nghiệm'', ''Tầng 2, Tòa nhà Kỹ thuật'', ''PGS.TS. Đỗ Trung Phấn'');
INSERT INTO Doctors (DoctorID, FirstName, LastName, Specialization, DepartmentID, ContactNumber, Email, JoiningDate)
VALUES 
(1, ''Nguyễn'', ''Quang Tuấn'', ''Tim mạch can thiệp'', 1, ''0912345678'', ''nguyenquangtuan@hospital.com'', ''2010-05-10''),
(2, ''Trần'', ''Thúy Hải'', ''Nhịp tim học'', 1, ''0923456789'', ''tranthuyhai@hospital.com'', ''2012-03-15''),
(3, ''Lê'', ''Xuân Thận'', ''Hồi sức tim mạch'', 3, ''0934567890'', ''lexuanthan@hospital.com'', ''2015-07-20''),
(4, ''Phạm'', ''Minh Tuấn'', ''Chẩn đoán hình ảnh tim mạch'', 4, ''0945678901'', ''phamminhtuan@hospital.com'', ''2016-09-25''),
(5, ''Hoàng'', ''Thị Lan'', ''Nội tiết - Đái tháo đường'', 2, ''0956789012'', ''hoangthilan@hospital.com'', ''2014-11-30''),
(6, ''Vũ'', ''Đình Hải'', ''Tim mạch người cao tuổi'', 1, ''0967890123'', ''vudinhhai@hospital.com'', ''2013-04-18''),
(7, ''Đỗ'', ''Thị Mai'', ''Xét nghiệm sinh hóa'', 5, ''0978901234'', ''dothimai@hospital.com'', ''2017-06-22'');
INSERT INTO Patients (PatientID, FirstName, LastName, Gender, DateOfBirth, BloodGroup, ContactNumber, Address, RegistrationDate)
VALUES 
(1, ''Trần'', ''Văn Lâm'', ''Nam'', ''1950-04-15'', ''A+'', ''0912345678'', ''123 Lê Duẩn, Quận Hoàn Kiếm, Hà Nội'', ''2023-01-10''),
(2, ''Nguyễn'', ''Thị Hoa'', ''Nữ'', ''1955-08-20'', ''O-'', ''0923456789'', ''456 Nguyễn Trãi, Quận Thanh Xuân, Hà Nội'', ''2023-01-15''),
(3, ''Lê'', ''Minh Quân'', ''Nam'', ''1965-12-05'', ''B+'', ''0934567890'', ''789 Giảng Võ, Quận Ba Đình, Hà Nội'', ''2023-01-20''),
(4, ''Phạm'', ''Thị Lan'', ''Nữ'', ''1970-03-25'', ''AB+'', ''0945678901'', ''101 Tây Sơn, Quận Đống Đa, Hà Nội'', ''2023-01-25''),
(5, ''Hoàng'', ''Văn Hùng'', ''Nam'', ''1945-07-30'', ''A-'', ''0956789012'', ''202 Láng Hạ, Quận Đống Đa, Hà Nội'', ''2023-01-30''),
(6, ''Vũ'', ''Thị Minh'', ''Nữ'', ''1960-09-12'', ''O+'', ''0967890123'', ''303 Trần Duy Hưng, Quận Cầu Giấy, Hà Nội'', ''2023-02-05''),
(7, ''Đặng'', ''Văn Tuấn'', ''Nam'', ''1975-11-18'', ''B-'', ''0978901234'', ''404 Xuân Thủy, Quận Cầu Giấy, Hà Nội'', ''2023-02-10''),
(8, ''Ngô'', ''Thị Hương'', ''Nữ'', ''1980-02-22'', ''AB-'', ''0989012345'', ''505 Hoàng Quốc Việt, Quận Bắc Từ Liêm, Hà Nội'', ''2023-02-15'');
INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, AppointmentDate, Description, Status)
VALUES 
(1, 1, 1, ''2023-03-01 09:00:00'', ''Đau ngực, khó thở'', ''Completed''),
(2, 2, 5, ''2023-03-02 10:30:00'', ''Kiểm tra đường huyết'', ''Completed''),
(3, 3, 2, ''2023-03-03 14:00:00'', ''Rối loạn nhịp tim'', ''Scheduled''),
(4, 4, 1, ''2023-03-04 15:30:00'', ''Tăng huyết áp'', ''Completed''),
(5, 5, 6, ''2023-03-05 08:00:00'', ''Theo dõi sau đặt stent'', ''Scheduled''),
(6, 6, 5, ''2023-03-06 11:00:00'', ''Tiểu nhiều, khát nước'', ''Cancelled''),
(7, 7, 3, ''2023-03-07 13:30:00'', ''Đau ngực cấp'', ''Completed''),
(8, 8, 4, ''2023-03-08 16:00:00'', ''Chụp CT mạch vành'', ''Scheduled''),
(9, 1, 2, ''2023-03-10 09:30:00'', ''Tái khám rối loạn nhịp'', ''Scheduled''),
(10, 3, 6, ''2023-03-12 10:00:00'', ''Theo dõi điều trị'', ''Scheduled'');
INSERT INTO Medications (MedicationID, MedicationName, Description, UnitPrice)
VALUES 
(1, ''Aspirin'', ''Thuốc chống đông máu'', 5000),
(2, ''Atorvastatin'', ''Thuốc hạ mỡ máu'', 25000),
(3, ''Metoprolol'', ''Thuốc chẹn beta'', 20000),
(4, ''Amlodipine'', ''Thuốc chẹn kênh canxi'', 18000),
(5, ''Clopidogrel'', ''Thuốc chống kết tập tiểu cầu'', 30000),
(6, ''Metformin'', ''Thuốc điều trị đái tháo đường'', 15000),
(7, ''Furosemide'', ''Thuốc lợi tiểu'', 10000),
(8, ''Nitroglycerin'', ''Thuốc giãn mạch'', 35000),
(9, ''Ramipril'', ''Thuốc ức chế men chuyển'', 22000),
(10, ''Insulin'', ''Hormone điều trị đái tháo đường'', 150000);
INSERT INTO Prescriptions (PrescriptionID, AppointmentID, MedicationID, Dosage, Frequency, StartDate, EndDate)
VALUES 
(1, 1, 1, ''100mg'', ''1 lần/ngày'', ''2023-03-01'', ''2023-03-31''),
(2, 1, 2, ''20mg'', ''1 lần/ngày, buổi tối'', ''2023-03-01'', ''2023-03-31''),
(3, 1, 3, ''25mg'', ''2 lần/ngày'', ''2023-03-01'', ''2023-03-31''),
(4, 2, 6, ''500mg'', ''2 lần/ngày, sau bữa ăn'', ''2023-03-02'', ''2023-03-16''),
(5, 4, 4, ''5mg'', ''1 lần/ngày'', ''2023-03-04'', ''2023-03-18''),
(6, 4, 9, ''5mg'', ''1 lần/ngày'', ''2023-03-04'', ''2023-03-18''),
(7, 5, 1, ''100mg'', ''1 lần/ngày'', ''2023-03-05'', ''2023-06-05''),
(8, 5, 5, ''75mg'', ''1 lần/ngày'', ''2023-03-05'', ''2023-06-05''),
(9, 7, 8, ''0.5mg'', ''Ngậm dưới lưỡi khi đau ngực'', ''2023-03-07'', ''2023-03-14''),
(10, 7, 3, ''50mg'', ''2 lần/ngày'', ''2023-03-07'', ''2023-03-21''),
(11, 7, 7, ''40mg'', ''1 lần/ngày, buổi sáng'', ''2023-03-07'', ''2023-03-14'');
--Bộ dữ liệu mẫu chi tiết cho khoa Tim mạch với các bệnh nhân mắc bệnh tim mạch và đái tháo đường