DELETE FROM OrderProduct;
DELETE FROM ComputerEmployee;
DELETE FROM EmployeeTraining;
DELETE FROM Employee;
DELETE FROM TrainingProgram;
DELETE FROM Computer;
DELETE FROM Department;
DELETE FROM [Order];
DELETE FROM PaymentType;
DELETE FROM Product;
DELETE FROM ProductType;
DELETE FROM Customer;


ALTER TABLE Employee DROP CONSTRAINT [FK_EmployeeDepartment];
ALTER TABLE ComputerEmployee DROP CONSTRAINT [FK_ComputerEmployee_Employee];
ALTER TABLE ComputerEmployee DROP CONSTRAINT [FK_ComputerEmployee_Computer];
ALTER TABLE EmployeeTraining DROP CONSTRAINT [FK_EmployeeTraining_Employee];
ALTER TABLE EmployeeTraining DROP CONSTRAINT [FK_EmployeeTraining_Training];
ALTER TABLE Product DROP CONSTRAINT [FK_Product_ProductType];
ALTER TABLE Product DROP CONSTRAINT [FK_Product_Customer];
ALTER TABLE PaymentType DROP CONSTRAINT [FK_PaymentType_Customer];
ALTER TABLE [Order] DROP CONSTRAINT [FK_Order_Customer];
ALTER TABLE [Order] DROP CONSTRAINT [FK_Order_Payment];
ALTER TABLE OrderProduct DROP CONSTRAINT [FK_OrderProduct_Product];
ALTER TABLE OrderProduct DROP CONSTRAINT [FK_OrderProduct_Order];


DROP TABLE IF EXISTS OrderProduct;
DROP TABLE IF EXISTS ComputerEmployee;
DROP TABLE IF EXISTS EmployeeTraining;
DROP TABLE IF EXISTS Employee;
DROP TABLE IF EXISTS TrainingProgram;
DROP TABLE IF EXISTS Computer;
DROP TABLE IF EXISTS Department;
DROP TABLE IF EXISTS [Order];
DROP TABLE IF EXISTS PaymentType;
DROP TABLE IF EXISTS Product;
DROP TABLE IF EXISTS ProductType;
DROP TABLE IF EXISTS Customer;


CREATE TABLE Department (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(55) NOT NULL,
	Budget 	INTEGER NOT NULL
);

CREATE TABLE Employee (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	FirstName VARCHAR(55) NOT NULL,
	LastName VARCHAR(55) NOT NULL,
	DepartmentId INTEGER NOT NULL,
	IsSuperVisor BIT NOT NULL DEFAULT(0),
    CONSTRAINT FK_EmployeeDepartment FOREIGN KEY(DepartmentId) REFERENCES Department(Id)
);

--CREATE TABLE Computer (
--	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
--	PurchaseDate DATETIME NOT NULL,
--	DecomissionDate DATETIME,
--	Make VARCHAR(55) NOT NULL,
--	Manufacturer VARCHAR(55) NOT NULL,
--	isArchived BIT NOT NULL DEFAULT(0)
--);

CREATE TABLE ComputerEmployee (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	EmployeeId INTEGER NOT NULL,
	ComputerId INTEGER NOT NULL,
	AssignDate DATETIME NOT NULL,
	UnassignDate DATETIME,
    CONSTRAINT FK_ComputerEmployee_Employee FOREIGN KEY(EmployeeId) REFERENCES Employee(Id),
    CONSTRAINT FK_ComputerEmployee_Computer FOREIGN KEY(ComputerId) REFERENCES Computer(Id)
);


CREATE TABLE TrainingProgram (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(255) NOT NULL,
	StartDate DATETIME NOT NULL,
	EndDate DATETIME NOT NULL,
	MaxAttendees INTEGER NOT NULL
);

CREATE TABLE EmployeeTraining (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	EmployeeId INTEGER NOT NULL,
	TrainingProgramId INTEGER NOT NULL,
    CONSTRAINT FK_EmployeeTraining_Employee FOREIGN KEY(EmployeeId) REFERENCES Employee(Id),
    CONSTRAINT FK_EmployeeTraining_Training FOREIGN KEY(TrainingProgramId) REFERENCES TrainingProgram(Id)
);

CREATE TABLE ProductType (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(55) NOT NULL,
	isArchived BIT NOT NULL DEFAULT(0)
);

CREATE TABLE Customer (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	FirstName VARCHAR(55) NOT NULL,
	LastName VARCHAR(55) NOT NULL
);

CREATE TABLE Product (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	ProductTypeId INTEGER NOT NULL,
	CustomerId INTEGER NOT NULL,
	Price INTEGER NOT NULL,
	Title VARCHAR(255) NOT NULL,
	[Description] VARCHAR(255) NOT NULL,
	Quantity INTEGER NOT NULL,
    CONSTRAINT FK_Product_ProductType FOREIGN KEY(ProductTypeId) REFERENCES ProductType(Id),
    CONSTRAINT FK_Product_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id),
	isArchived BIT NOT NULL DEFAULT(0), 
);


CREATE TABLE PaymentType (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	AcctNumber INTEGER NOT NULL,
	[Name] VARCHAR(55) NOT NULL,
	CustomerId INTEGER NOT NULL,
    CONSTRAINT FK_PaymentType_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id),
	isArchived BIT NOT NULL DEFAULT(0), 
);

CREATE TABLE [Order] (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	CustomerId INTEGER NOT NULL,
	PaymentTypeId INTEGER,
    CONSTRAINT FK_Order_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id),
    CONSTRAINT FK_Order_Payment FOREIGN KEY(PaymentTypeId) REFERENCES PaymentType(Id)
);

CREATE TABLE OrderProduct (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	OrderId INTEGER NOT NULL,
	ProductId INTEGER NOT NULL,
    CONSTRAINT FK_OrderProduct_Product FOREIGN KEY(ProductId) REFERENCES Product(Id),
    CONSTRAINT FK_OrderProduct_Order FOREIGN KEY(OrderId) REFERENCES [Order](Id)
);


INSERT INTO Department (Name, Budget) Values ('Accounting', 20000);
INSERT INTO Department (Name, Budget) Values ('Marketing', 25000);
INSERT INTO Department (Name, Budget) Values ('Logistics', 30000);

INSERT INTO Employee (FirstName, LastName, DepartmentId, IsSuperVisor) Values ('Larry', 'Bird', 1, 0);
INSERT INTO Employee (FirstName, LastName, DepartmentId, IsSuperVisor) Values ('Earl', 'Povich', 3, 0);
INSERT INTO Employee (FirstName, LastName, DepartmentId, IsSuperVisor) Values ('Stacy', 'Blackwell', 2, 0);
INSERT INTO Employee (FirstName, LastName, DepartmentId, IsSuperVisor) Values ('Amy', 'Early', 1, 0);

INSERT INTO Computer (PurchaseDate, DecomissionDate, Make, Manufacturer, isArchived) Values ('20190222', null, 'Inspiron 7000', 'Dell', 0);
INSERT INTO Computer (PurchaseDate, DecomissionDate, Make, Manufacturer, isArchived) Values ('20190131', null, 'Inspiron 7999', 'Dell', 0);
INSERT INTO Computer (PurchaseDate, DecomissionDate, Make, Manufacturer, isArchived) Values ('20180428', null, 'MacBook Pro', 'Apple', 0);

INSERT INTO ComputerEmployee (EmployeeId, ComputerId, AssignDate, UnassignDate) Values (1, 2, '20190202', null);
INSERT INTO ComputerEmployee (EmployeeId, ComputerId, AssignDate, UnassignDate) Values (2, 1, '20190223', null);
INSERT INTO ComputerEmployee (EmployeeId, ComputerId, AssignDate, UnassignDate) Values (3, 3, '20190428', null);

INSERT INTO Customer (FirstName, LastName) VALUES ('Bob', 'Smith');
INSERT INTO Customer (FirstName, LastName) VALUES ('Jason', 'Hill');
INSERT INTO Customer (FirstName, LastName) VALUES ('Ray', 'Charles');
INSERT INTO Customer (FirstName, LastName) VALUES ('Owen', 'Wilson');
INSERT INTO Customer (FirstName, LastName) VALUES ('Jeff', 'Sucks');

INSERT INTO ProductType (Name, IsArchived) VALUES ('Product 1', 0);
INSERT INTO ProductType (Name, IsArchived) VALUES ('Product 2', 0);
INSERT INTO ProductType (Name, IsArchived) VALUES ('Product 3', 0);
INSERT INTO ProductType (Name, IsArchived) VALUES ('Product 4', 0);
INSERT INTO ProductType (Name, IsArchived) VALUES ('Product 5', 0);

INSERT INTO Product (Price, Title, Description, Quantity, IsArchived, ProductTypeId, CustomerId) VALUES (15, 'Item 1', 'This is the description for this item', 5, 'False', 1, 2);
INSERT INTO Product (Price, Title, Description, Quantity, IsArchived, ProductTypeId, CustomerId) VALUES (15, 'Item 2', 'This is the description for this item', 4, 'False', 2, 1);
INSERT INTO Product (Price, Title, Description, Quantity, IsArchived, ProductTypeId, CustomerId) VALUES (15, 'Item 3', 'This is the description for this item', 3, 'False', 3, 1);
INSERT INTO Product (Price, Title, Description, Quantity, IsArchived, ProductTypeId, CustomerId) VALUES (15, 'Item 4', 'This is the description for this item', 2, 'False', 2, 3);
INSERT INTO Product (Price, Title, Description, Quantity, IsArchived, ProductTypeId, CustomerId) VALUES (15, 'Item 1', 'This is the description for this item', 5, 'False', 4, 4);

INSERT INTO PaymentType (AcctNumber, Name, CustomerId, IsArchived) VALUES (11111, 'Visa', '1', 0);
INSERT INTO PaymentType (AcctNumber, Name, CustomerId, IsArchived) VALUES (11111, 'Debot', '2', 0);
INSERT INTO PaymentType (AcctNumber, Name, CustomerId, IsArchived) VALUES (11111, 'Visa', '3', 0);
INSERT INTO PaymentType (AcctNumber, Name, CustomerId, IsArchived) VALUES (11111, 'Debit', '4', 0);
INSERT INTO PaymentType (AcctNumber, Name, CustomerId, IsArchived) VALUES (11111, 'Visa', '5', 0);

INSERT INTO [Order] (PaymentTypeId, CustomerId) VALUES (1, 1);
INSERT INTO [Order] (PaymentTypeId, CustomerId) VALUES (2, 2);
INSERT INTO [Order] (PaymentTypeId, CustomerId) VALUES (3, 3);
INSERT INTO [Order] (PaymentTypeId, CustomerId) VALUES (4, 4);

INSERT INTO OrderProduct(OrderId, ProductId) VALUES (1, 2);
INSERT INTO OrderProduct(OrderId, ProductId) VALUES (1, 2);
INSERT INTO OrderProduct(OrderId, ProductId) VALUES (2, 3);
INSERT INTO OrderProduct(OrderId, ProductId) VALUES (3, 4);
INSERT INTO OrderProduct(OrderId, ProductId) VALUES (4, 5);
INSERT INTO OrderProduct(OrderId, ProductId) VALUES (5, 6);
