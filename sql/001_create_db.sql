IF DB_ID(N'CinemaDb') IS NULL
BEGIN
    CREATE DATABASE CinemaDb;
END
GO

USE CinemaDb;
GO

IF OBJECT_ID(N'dbo.Tickets', N'U') IS NOT NULL DROP TABLE dbo.Tickets;
IF OBJECT_ID(N'dbo.Showtimes', N'U') IS NOT NULL DROP TABLE dbo.Showtimes;
IF OBJECT_ID(N'dbo.Seats', N'U') IS NOT NULL DROP TABLE dbo.Seats;
IF OBJECT_ID(N'dbo.Auditoriums', N'U') IS NOT NULL DROP TABLE dbo.Auditoriums;
IF OBJECT_ID(N'dbo.Movies', N'U') IS NOT NULL DROP TABLE dbo.Movies;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL DROP TABLE dbo.Roles;
GO

CREATE TABLE dbo.Roles
(
    RoleId      INT             IDENTITY(1,1) PRIMARY KEY,
    RoleName    NVARCHAR(50)    NOT NULL UNIQUE
);
GO

CREATE TABLE dbo.Users
(
    UserId       INT              IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)     NOT NULL UNIQUE,
    PasswordHash NVARCHAR(64)     NOT NULL,
    RoleId       INT              NOT NULL,
    FullName     NVARCHAR(100)    NULL,
    Email        NVARCHAR(255)    NULL,
    CreatedAt    DATETIME2(0)     NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
);
GO

CREATE TABLE dbo.Movies
(
    MovieId     INT              IDENTITY(1,1) PRIMARY KEY,
    Title       NVARCHAR(200)    NOT NULL UNIQUE,
    Description NVARCHAR(MAX)    NULL,
    Duration    INT              NOT NULL,
    ReleaseDate DATE             NULL,
    Rating      NVARCHAR(10)     NULL,
    CONSTRAINT CK_Movies_Duration_Positive CHECK (Duration > 0)
);
GO

CREATE TABLE dbo.Auditoriums
(
    AuditoriumId INT             IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(100)   NOT NULL UNIQUE,
    SeatRows     INT             NOT NULL,
    SeatCols     INT             NOT NULL,
    Location     NVARCHAR(200)   NULL,
    CONSTRAINT CK_Auditoriums_SeatRows_Positive CHECK (SeatRows > 0),
    CONSTRAINT CK_Auditoriums_SeatCols_Positive CHECK (SeatCols > 0)
);
GO

CREATE TABLE dbo.Seats
(
    SeatId        INT          IDENTITY(1,1) PRIMARY KEY,
    AuditoriumId  INT          NOT NULL,
    RowNumber     INT          NOT NULL,
    ColumnNumber  INT          NOT NULL,
    CONSTRAINT FK_Seats_Auditoriums FOREIGN KEY (AuditoriumId) REFERENCES dbo.Auditoriums(AuditoriumId) ON DELETE CASCADE,
    CONSTRAINT CK_Seats_Row_Positive CHECK (RowNumber > 0),
    CONSTRAINT CK_Seats_Column_Positive CHECK (ColumnNumber > 0),
    CONSTRAINT UQ_Seats_Auditorium_Row_Column UNIQUE (AuditoriumId, RowNumber, ColumnNumber)
);
GO

CREATE TABLE dbo.Showtimes
(
    ShowtimeId    INT             IDENTITY(1,1) PRIMARY KEY,
    MovieId       INT             NOT NULL,
    AuditoriumId  INT             NOT NULL,
    StartTime     DATETIME2(0)    NOT NULL,
    EndTime       DATETIME2(0)    NOT NULL,
    BasePrice     DECIMAL(10,2)   NOT NULL,
    CONSTRAINT FK_Showtimes_Movies FOREIGN KEY (MovieId) REFERENCES dbo.Movies(MovieId),
    CONSTRAINT FK_Showtimes_Auditoriums FOREIGN KEY (AuditoriumId) REFERENCES dbo.Auditoriums(AuditoriumId),
    CONSTRAINT CK_Showtimes_TimeRange CHECK (EndTime > StartTime),
    CONSTRAINT CK_Showtimes_BasePrice_Positive CHECK (BasePrice > 0)
);
GO

CREATE TABLE dbo.Tickets
(
    TicketId     INT             IDENTITY(1,1) PRIMARY KEY,
    ShowtimeId   INT             NOT NULL,
    SeatId       INT             NOT NULL,
    TicketPrice  DECIMAL(10,2)   NOT NULL,
    SoldAt       DATETIME2(0)    NOT NULL CONSTRAINT DF_Tickets_SoldAt DEFAULT SYSUTCDATETIME(),
    BuyerName    NVARCHAR(100)   NULL,
    CONSTRAINT FK_Tickets_Showtimes FOREIGN KEY (ShowtimeId) REFERENCES dbo.Showtimes(ShowtimeId) ON DELETE CASCADE,
    CONSTRAINT FK_Tickets_Seats FOREIGN KEY (SeatId) REFERENCES dbo.Seats(SeatId),
    CONSTRAINT CK_Tickets_TicketPrice_Positive CHECK (TicketPrice > 0),
    CONSTRAINT UQ_Tickets_Showtime_Seat UNIQUE (ShowtimeId, SeatId)
);
GO

IF OBJECT_ID(N'dbo.vw_RevenueDaily', N'V') IS NOT NULL
    DROP VIEW dbo.vw_RevenueDaily;
GO

CREATE VIEW dbo.vw_RevenueDaily
AS
SELECT
    CAST(T.SoldAt AS DATE) AS SaleDate,
    SUM(T.TicketPrice) AS TotalRevenue,
    COUNT(*) AS TotalTickets
FROM dbo.Tickets AS T
GROUP BY CAST(T.SoldAt AS DATE);
GO

IF OBJECT_ID(N'dbo.sp_TopMoviesByRevenue', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_TopMoviesByRevenue;
GO

CREATE PROCEDURE dbo.sp_TopMoviesByRevenue
    @From  DATE,
    @To    DATE,
    @TopN  INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @TopN IS NULL OR @TopN <= 0
    BEGIN
        RAISERROR('TopN must be greater than zero.', 16, 1);
        RETURN;
    END;

    SELECT TOP (@TopN)
        M.MovieId,
        M.Title,
        SUM(T.TicketPrice) AS TotalRevenue,
        COUNT(*) AS TotalTickets
    FROM dbo.Tickets AS T
    INNER JOIN dbo.Showtimes AS S ON T.ShowtimeId = S.ShowtimeId
    INNER JOIN dbo.Movies AS M ON S.MovieId = M.MovieId
    WHERE T.SoldAt >= @From AND T.SoldAt < DATEADD(DAY, 1, @To)
    GROUP BY M.MovieId, M.Title
    ORDER BY TotalRevenue DESC, TotalTickets DESC, M.Title ASC;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Admin')
BEGIN
    INSERT INTO dbo.Roles (RoleName)
    VALUES (N'Admin');
END
GO

DECLARE @AdminRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = N'Admin');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'admin')
BEGIN
    INSERT INTO dbo.Users (Username, PasswordHash, RoleId, FullName)
    VALUES (N'admin', N'21232f297a57a5a743894a0e4a801fc3', @AdminRoleId, N'System Administrator');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Movies)
BEGIN
    INSERT INTO dbo.Movies (Title, Description, Duration, ReleaseDate, Rating)
    VALUES (N'Sample Movie', N'A sample movie for initial data.', 120, CAST(GETDATE() AS DATE), N'PG-13');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Auditoriums WHERE Name = N'Room A')
BEGIN
    INSERT INTO dbo.Auditoriums (Name, SeatRows, SeatCols, Location)
    VALUES (N'Room A', 8, 12, N'First Floor');
END
GO

DECLARE @AuditoriumId INT = (SELECT AuditoriumId FROM dbo.Auditoriums WHERE Name = N'Room A');
DECLARE @SeatRows INT = (SELECT SeatRows FROM dbo.Auditoriums WHERE AuditoriumId = @AuditoriumId);
DECLARE @SeatCols INT = (SELECT SeatCols FROM dbo.Auditoriums WHERE AuditoriumId = @AuditoriumId);

IF NOT EXISTS (SELECT 1 FROM dbo.Seats WHERE AuditoriumId = @AuditoriumId)
BEGIN
    DECLARE @Row INT = 1;
    WHILE @Row <= @SeatRows
    BEGIN
        DECLARE @Col INT = 1;
        WHILE @Col <= @SeatCols
        BEGIN
            INSERT INTO dbo.Seats (AuditoriumId, RowNumber, ColumnNumber)
            VALUES (@AuditoriumId, @Row, @Col);
            SET @Col += 1;
        END;
        SET @Row += 1;
    END;
END
GO
