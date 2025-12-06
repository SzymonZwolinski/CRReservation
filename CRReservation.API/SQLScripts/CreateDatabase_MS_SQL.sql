-- MS SQL Server database creation script for CRReservation
-- Run this script in SQL Server Management Studio or similar tool

USE master;
GO

-- Create database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SalaRezerwacja')
BEGIN
    CREATE DATABASE SalaRezerwacja;
END
GO

USE SalaRezerwacja;
GO

-- Create Role table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Role' AND xtype='U')
BEGIN
    CREATE TABLE [Role] (
        Name NVARCHAR(20) PRIMARY KEY,
        Description NVARCHAR(100) NOT NULL
    );
END
GO

-- Create ClassRoom table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ClassRooms' AND xtype='U')
BEGIN
    CREATE TABLE ClassRooms (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100) NOT NULL,
        Capacity INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        Notes NVARCHAR(500) NULL
    );
END
GO

-- Create User table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id INT PRIMARY KEY IDENTITY(1,1),
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        RoleName NVARCHAR(20) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        FOREIGN KEY (RoleName) REFERENCES [Role](Name)
    );
END
GO

-- Create Group table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Groups' AND xtype='U')
BEGIN
    CREATE TABLE Groups (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL
    );
END
GO

-- Create UserGroup table (many-to-many relationship)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserGroups' AND xtype='U')
BEGIN
    CREATE TABLE UserGroups (
        Id INT PRIMARY KEY IDENTITY(1,1),
        GroupId INT NOT NULL,
        UserId INT NOT NULL,
        FOREIGN KEY (GroupId) REFERENCES Groups(Id) ON DELETE CASCADE,
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        UNIQUE (GroupId, UserId)
    );
END
GO

-- Create Reservation table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reservations' AND xtype='U')
BEGIN
    CREATE TABLE Reservations (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Status NVARCHAR(20) NOT NULL DEFAULT 'oczekujaca',
        ClassRoomId INT NOT NULL,
        ReservationDate DATE NOT NULL,
        GroupId INT NULL,
        IsRecurring BIT NOT NULL DEFAULT 0,
        StartDateTime DATETIME NOT NULL,
        EndDateTime DATETIME NOT NULL,
        UserId INT NOT NULL,
        FOREIGN KEY (ClassRoomId) REFERENCES ClassRooms(Id) ON DELETE CASCADE,
        FOREIGN KEY (GroupId) REFERENCES Groups(Id) ON DELETE SET NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION,
        CHECK (EndDateTime > StartDateTime)
    );
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Reservations_ReservationDate_ClassRoomId')
BEGIN
    CREATE INDEX IX_Reservations_ReservationDate_ClassRoomId ON Reservations(ReservationDate, ClassRoomId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Reservations_UserId')
BEGIN
    CREATE INDEX IX_Reservations_UserId ON Reservations(UserId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_UserGroups_GroupId_UserId')
BEGIN
    CREATE INDEX IX_UserGroups_GroupId_UserId ON UserGroups(GroupId, UserId);
END
GO

-- Seed data
-- Insert roles
IF NOT EXISTS (SELECT * FROM [Role] WHERE Name = 'admin')
BEGIN
    INSERT INTO [Role] (Name, Description) VALUES
        ('admin', 'Administrator systemu'),
        ('prowadzacy', 'Prowadzący zajęcia'),
        ('student', 'Student');
END
GO

-- Insert sample classrooms
IF NOT EXISTS (SELECT * FROM ClassRooms WHERE Name = 'Sala 101')
BEGIN
    INSERT INTO ClassRooms (Name, Capacity, IsActive) VALUES
        ('Sala 101', 30, 1),
        ('Sala 202', 50, 1),
        ('Sala 303', 20, 1);
END
GO

-- Insert sample users (including admin)
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'jan.kowalski@example.com')
BEGIN
    INSERT INTO Users (FirstName, LastName, RoleName, Email) VALUES
        ('Jan', 'Kowalski', 'admin', 'jan.kowalski@example.com'),
        ('Anna', 'Nowak', 'prowadzacy', 'anna.nowak@example.com'),
        ('Piotr', 'Wiśniewski', 'student', 'piotr.wisniewski@example.com');
END
GO

-- Insert sample groups
IF NOT EXISTS (SELECT * FROM Groups WHERE Name = 'Informatyka I rok')
BEGIN
    INSERT INTO Groups (Name, Description) VALUES
        ('Informatyka I rok', 'Studenci pierwszego roku informatyki'),
        ('Zarządzanie II rok', 'Studenci drugiego roku zarządzania');
END
GO

-- Insert user-group relationships
IF NOT EXISTS (SELECT * FROM UserGroups WHERE UserId = 3 AND GroupId = 1)
BEGIN
    INSERT INTO UserGroups (GroupId, UserId) VALUES
        (1, 3), -- Piotr in Informatyka I rok
        (2, 3); -- Piotr in Zarządzanie II rok
END
GO

-- Insert sample reservations
IF NOT EXISTS (SELECT * FROM Reservations WHERE Id = 1)
BEGIN
    INSERT INTO Reservations (Status, ClassRoomId, ReservationDate, GroupId, IsRecurring, StartDateTime, EndDateTime, UserId)
    VALUES
        ('potwierdzona', 1, '2025-12-01', 1, 0, '2025-12-01 10:00:00', '2025-12-01 12:00:00', 2),
        ('oczekujaca', 2, '2025-12-02', 2, 1, '2025-12-02 14:00:00', '2025-12-02 16:00:00', 2);
END
GO

PRINT 'Database SalaRezerwacja created and seeded successfully!';
GO
