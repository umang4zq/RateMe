-- ============================================================================
-- RateMe Database Schema & Seed Data Script
-- Microsoft SQL Server / LocalDB (T-SQL)
-- Target: Visual Studio 2019 / SQL Server 2016+ / (localdb)\MSSQLLocalDB
-- ============================================================================

USE [master];
GO

-- 1. Create Database if it doesn't already exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'RateMeDb')
BEGIN
    CREATE DATABASE [RateMeDb];
    PRINT 'Database [RateMeDb] created successfully.';
END
ELSE
BEGIN
    PRINT 'Database [RateMeDb] already exists.';
END
GO

USE [RateMeDb];
GO

-- ============================================================================
-- 2. Drop existing foreign keys and tables if running a full reset (safe check)
-- ============================================================================

IF OBJECT_ID(N'dbo.UserSettings', N'U') IS NOT NULL DROP TABLE dbo.UserSettings;
IF OBJECT_ID(N'dbo.Watchlist', N'U') IS NOT NULL DROP TABLE dbo.Watchlist;
IF OBJECT_ID(N'dbo.Ratings', N'U') IS NOT NULL DROP TABLE dbo.Ratings;
IF OBJECT_ID(N'dbo.MediaItems', N'U') IS NOT NULL DROP TABLE dbo.MediaItems;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- ============================================================================
-- 3. Users Table
-- Supports secure password hashing (PBKDF2/SHA-256 + Salt) and Role-Based Access
-- ============================================================================
CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY CLUSTERED,
    Username NVARCHAR(50) NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
    Email NVARCHAR(100) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Salt NVARCHAR(64) NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    Bio NVARCHAR(500) NULL,
    AvatarUrl NVARCHAR(500) NULL,
    Role NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT (N'User'), -- 'User', 'Admin'
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (GETDATE()),
    LastLoginAt DATETIME NULL
);
GO

CREATE NONCLUSTERED INDEX IX_Users_Role ON dbo.Users(Role);
CREATE NONCLUSTERED INDEX IX_Users_IsActive ON dbo.Users(IsActive);
GO

-- ============================================================================
-- 4. MediaItems Table (Movies & Series Catalog / Cache)
-- ============================================================================
CREATE TABLE dbo.MediaItems (
    MediaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MediaItems PRIMARY KEY CLUSTERED,
    ImdbId NVARCHAR(30) NOT NULL CONSTRAINT UQ_MediaItems_ImdbId UNIQUE,
    Title NVARCHAR(255) NOT NULL,
    Year NVARCHAR(20) NULL,
    Type NVARCHAR(20) NOT NULL CONSTRAINT DF_MediaItems_Type DEFAULT (N'movie'), -- 'movie', 'series'
    Genre NVARCHAR(200) NULL,
    Director NVARCHAR(200) NULL,
    Actors NVARCHAR(500) NULL,
    Plot NVARCHAR(MAX) NULL,
    PosterUrl NVARCHAR(1000) NULL,
    ImdbRating DECIMAL(3,1) NULL,
    Metascore INT NULL,
    Runtime NVARCHAR(50) NULL,
    TotalUserRatings INT NOT NULL CONSTRAINT DF_MediaItems_TotalRatings DEFAULT (0),
    AverageUserRating DECIMAL(3,2) NOT NULL CONSTRAINT DF_MediaItems_AvgRating DEFAULT (0.00),
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_MediaItems_CreatedAt DEFAULT (GETDATE()),
    UpdatedAt DATETIME NOT NULL CONSTRAINT DF_MediaItems_UpdatedAt DEFAULT (GETDATE())
);
GO

CREATE NONCLUSTERED INDEX IX_MediaItems_Title ON dbo.MediaItems(Title);
CREATE NONCLUSTERED INDEX IX_MediaItems_Type ON dbo.MediaItems(Type);
CREATE NONCLUSTERED INDEX IX_MediaItems_ImdbRating ON dbo.MediaItems(ImdbRating DESC);
CREATE NONCLUSTERED INDEX IX_MediaItems_AvgUserRating ON dbo.MediaItems(AverageUserRating DESC);
GO

-- ============================================================================
-- 5. Ratings Table
-- ============================================================================
CREATE TABLE dbo.Ratings (
    RatingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Ratings PRIMARY KEY CLUSTERED,
    UserId INT NOT NULL CONSTRAINT FK_Ratings_Users FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    MediaId INT NOT NULL CONSTRAINT FK_Ratings_MediaItems FOREIGN KEY REFERENCES dbo.MediaItems(MediaId) ON DELETE CASCADE,
    Score DECIMAL(3,1) NOT NULL CONSTRAINT CK_Ratings_Score CHECK (Score >= 0.5 AND Score <= 10.0),
    ReviewText NVARCHAR(MAX) NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Ratings_CreatedAt DEFAULT (GETDATE()),
    UpdatedAt DATETIME NOT NULL CONSTRAINT DF_Ratings_UpdatedAt DEFAULT (GETDATE()),
    CONSTRAINT UQ_Ratings_User_Media UNIQUE (UserId, MediaId)
);
GO

CREATE NONCLUSTERED INDEX IX_Ratings_UserId ON dbo.Ratings(UserId);
CREATE NONCLUSTERED INDEX IX_Ratings_MediaId ON dbo.Ratings(MediaId);
CREATE NONCLUSTERED INDEX IX_Ratings_Score ON dbo.Ratings(Score DESC);
GO

-- ============================================================================
-- 6. Watchlist Table
-- ============================================================================
CREATE TABLE dbo.Watchlist (
    WatchlistId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Watchlist PRIMARY KEY CLUSTERED,
    UserId INT NOT NULL CONSTRAINT FK_Watchlist_Users FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    MediaId INT NOT NULL CONSTRAINT FK_Watchlist_MediaItems FOREIGN KEY REFERENCES dbo.MediaItems(MediaId) ON DELETE CASCADE,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Watchlist_Status DEFAULT (N'PlanToWatch'), -- 'PlanToWatch', 'Watching', 'Completed', 'Dropped'
    AddedAt DATETIME NOT NULL CONSTRAINT DF_Watchlist_AddedAt DEFAULT (GETDATE()),
    CONSTRAINT UQ_Watchlist_User_Media UNIQUE (UserId, MediaId)
);
GO

CREATE NONCLUSTERED INDEX IX_Watchlist_UserId ON dbo.Watchlist(UserId);
CREATE NONCLUSTERED INDEX IX_Watchlist_Status ON dbo.Watchlist(Status);
GO

-- ============================================================================
-- 7. UserSettings Table
-- ============================================================================
CREATE TABLE dbo.UserSettings (
    SettingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserSettings PRIMARY KEY CLUSTERED,
    UserId INT NOT NULL CONSTRAINT FK_UserSettings_Users FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    DarkMode BIT NOT NULL CONSTRAINT DF_UserSettings_DarkMode DEFAULT (0),
    PreferredGenre NVARCHAR(100) NULL,
    NotificationsEnabled BIT NOT NULL CONSTRAINT DF_UserSettings_Notifications DEFAULT (1),
    CustomApiKey NVARCHAR(100) NULL,
    UpdatedAt DATETIME NOT NULL CONSTRAINT DF_UserSettings_UpdatedAt DEFAULT (GETDATE()),
    CONSTRAINT UQ_UserSettings_UserId UNIQUE (UserId)
);
GO

-- ============================================================================
-- 8. Trigger to maintain MediaItems Average Rating & Total Ratings
-- ============================================================================
CREATE OR ALTER TRIGGER trg_Ratings_UpdateStats
ON dbo.Ratings
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @AffectedMedia TABLE (MediaId INT);
    
    INSERT INTO @AffectedMedia (MediaId)
    SELECT DISTINCT MediaId FROM inserted
    UNION
    SELECT DISTINCT MediaId FROM deleted;
    
    UPDATE m
    SET 
        TotalUserRatings = ISNULL(stats.TotalRatings, 0),
        AverageUserRating = ISNULL(stats.AvgScore, 0.0),
        UpdatedAt = GETDATE()
    FROM dbo.MediaItems m
    INNER JOIN @AffectedMedia aff ON m.MediaId = aff.MediaId
    OUTER APPLY (
        SELECT 
            COUNT(1) AS TotalRatings,
            ROUND(AVG(Score), 2) AS AvgScore
        FROM dbo.Ratings r
        WHERE r.MediaId = m.MediaId
    ) stats;
END;
GO

-- ============================================================================
-- 9. Views for Reporting and Dashboard Summaries
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_MediaRatingSummary
AS
SELECT 
    m.MediaId,
    m.ImdbId,
    m.Title,
    m.Year,
    m.Type,
    m.Genre,
    m.PosterUrl,
    m.ImdbRating,
    m.TotalUserRatings,
    m.AverageUserRating,
    COUNT(w.WatchlistId) AS WatchlistCount
FROM dbo.MediaItems m
LEFT JOIN dbo.Watchlist w ON m.MediaId = w.MediaId
GROUP BY 
    m.MediaId, m.ImdbId, m.Title, m.Year, m.Type, m.Genre, 
    m.PosterUrl, m.ImdbRating, m.TotalUserRatings, m.AverageUserRating;
GO

CREATE OR ALTER VIEW dbo.vw_UserRecentRatings
AS
SELECT 
    r.RatingId,
    r.UserId,
    u.Username,
    u.DisplayName,
    m.MediaId,
    m.ImdbId,
    m.Title AS MediaTitle,
    m.PosterUrl,
    r.Score,
    r.ReviewText,
    r.CreatedAt AS RatedAt
FROM dbo.Ratings r
JOIN dbo.Users u ON r.UserId = u.UserId
JOIN dbo.MediaItems m ON r.MediaId = m.MediaId;
GO

-- ============================================================================
-- 10. Seed Initial Data
-- ============================================================================

INSERT INTO dbo.Users (Username, Email, PasswordHash, Salt, DisplayName, Bio, Role, IsActive)
VALUES 
(
    N'admin',
    N'admin@rateme.app',
    N'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3',
    N'salt_admin_demo_987654321',
    N'RateMe Director',
    N'Chief Curator and Administrator at RateMe Studio.',
    N'Admin',
    1
),
(
    N'cinema_fan',
    N'fan@rateme.app',
    N'8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',
    N'salt_user_demo_123456789',
    N'Aria Sterling',
    N'Film buff, coffee lover, and weekend series binger.',
    N'User',
    1
);

DECLARE @AdminId INT = (SELECT UserId FROM dbo.Users WHERE Username = N'admin');
DECLARE @UserId INT = (SELECT UserId FROM dbo.Users WHERE Username = N'cinema_fan');

INSERT INTO dbo.UserSettings (UserId, DarkMode, PreferredGenre, CustomApiKey)
VALUES 
(@AdminId, 0, N'Sci-Fi', N'cf367844'),
(@UserId, 0, N'Drama', N'cf367844');

-- B. Curated Initial Media Catalog with Official High-Res OMDb Posters
INSERT INTO dbo.MediaItems (ImdbId, Title, Year, Type, Genre, Director, Actors, Plot, PosterUrl, ImdbRating, Metascore, Runtime)
VALUES 
(
    N'tt1375666',
    N'Inception',
    N'2010',
    N'movie',
    N'Adventure, Sci-Fi, Thriller',
    N'Christopher Nolan',
    N'Leonardo DiCaprio, Joseph Gordon-Levitt, Elliot Page',
    N'A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea into the mind of a C.E.O., but his tragic past may doom the project and his team to disaster.',
    N'https://m.media-amazon.com/images/M/MV5BMjAxMzY3NjcxNF5BMl5BanBnXkFtZTcwNTI5OTM0Mw@@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    8.8,
    74,
    N'148 min'
),
(
    N'tt0468569',
    N'The Dark Knight',
    N'2008',
    N'movie',
    N'Crime, Thriller, Action',
    N'Christopher Nolan',
    N'Christian Bale, Heath Ledger, Aaron Eckhart',
    N'When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.',
    N'https://m.media-amazon.com/images/M/MV5BMTMxNTMwODM0NF5BMl5BanBnXkFtZTcwODAyMTk2Mw@@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    9.1,
    84,
    N'152 min'
),
(
    N'tt0816692',
    N'Interstellar',
    N'2014',
    N'movie',
    N'Adventure, Sci-Fi, Drama',
    N'Christopher Nolan',
    N'Matthew McConaughey, Anne Hathaway, Jessica Chastain',
    N'When Earth becomes uninhabitable in the future, a farmer and ex-NASA pilot, Joseph Cooper, is tasked to pilot a spacecraft, along with a team of researchers, to find a new planet for humans.',
    N'https://m.media-amazon.com/images/M/MV5BYzdjMDAxZGItMjI2My00ODA1LTlkNzItOWFjMDU5ZDJlYWY3XkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    8.7,
    74,
    N'169 min'
),
(
    N'tt0903747',
    N'Breaking Bad',
    N'2008–2013',
    N'series',
    N'Crime, Drama, Thriller',
    N'Vince Gilligan',
    N'Bryan Cranston, Aaron Paul, Anna Gunn',
    N'A chemistry teacher diagnosed with inoperable lung cancer turns to manufacturing and selling methamphetamine with a former student in order to secure his family''s financial future.',
    N'https://m.media-amazon.com/images/M/MV5BOWE4NTc3YmYtNmU2Mi00ZjhkLWE1MTItZmM1M2U1ODU3YjFlXkEyXkFqcGc@._V1_QL75_UY562_CR2,0,380,562_.jpg',
    9.5,
    NULL,
    N'49 min'
),
(
    N'tt4574334',
    N'Stranger Things',
    N'2016–2025',
    N'series',
    N'Drama, Fantasy, Horror',
    N'The Duffer Brothers',
    N'Millie Bobby Brown, Finn Wolfhard, Winona Ryder',
    N'When a young boy vanishes, a small town uncovers a mystery involving secret experiments, terrifying supernatural forces and one strange little girl.',
    N'https://m.media-amazon.com/images/M/MV5BMjEzMDAxOTUyMV5BMl5BanBnXkFtZTgwNzAxMzYzOTE@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    8.6,
    NULL,
    N'51 min'
),
(
    N'tt0111161',
    N'The Shawshank Redemption',
    N'1994',
    N'movie',
    N'Drama',
    N'Frank Darabont',
    N'Tim Robbins, Morgan Freeman, Bob Gunton',
    N'Over the course of several years, two convicts form a friendship, seeking consolation and, eventually, redemption through basic compassion.',
    N'https://m.media-amazon.com/images/M/MV5BMDAyY2FhYjctNDc5OS00MDNlLThiMGUtY2UxYWVkNGY2ZjljXkEyXkFqcGc@._V1_QL75_UX380_CR0,4,380,562_.jpg',
    9.3,
    82,
    N'142 min'
),
(
    N'tt0133093',
    N'The Matrix',
    N'1999',
    N'movie',
    N'Action, Sci-Fi',
    N'Lana Wachowski, Lilly Wachowski',
    N'Keanu Reeves, Laurence Fishburne, Carrie-Anne Moss',
    N'When a beautiful stranger leads computer hacker Neo to a forbidding underworld, he discovers the shocking truth--the life he knows is the elaborate deception of an evil cyber-intelligence.',
    N'https://m.media-amazon.com/images/M/MV5BN2NmN2VhMTQtMDNiOS00NDlhLTliMjgtODE2ZTY0ODQyNDRhXkEyXkFqcGc@._V1_QL75_UX380_CR0,4,380,562_.jpg',
    8.7,
    73,
    N'136 min'
),
(
    N'tt15398776',
    N'Oppenheimer',
    N'2023',
    N'movie',
    N'Biography, Drama, History',
    N'Christopher Nolan',
    N'Cillian Murphy, Emily Blunt, Matt Damon',
    N'The story of American scientist J. Robert Oppenheimer and his role in the development of the atomic bomb during World War II.',
    N'https://m.media-amazon.com/images/M/MV5BN2JkMDc5MGQtZjg3YS00NmFiLWIyZmQtZTJmNTM5MjVmYTQ4XkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    8.2,
    88,
    N'180 min'
);

-- C. Sample Seed Ratings
DECLARE @MediaInception INT = (SELECT MediaId FROM dbo.MediaItems WHERE ImdbId = N'tt1375666');
DECLARE @MediaDarkKnight INT = (SELECT MediaId FROM dbo.MediaItems WHERE ImdbId = N'tt0468569');
DECLARE @MediaBreakingBad INT = (SELECT MediaId FROM dbo.MediaItems WHERE ImdbId = N'tt0903747');

INSERT INTO dbo.Ratings (UserId, MediaId, Score, ReviewText)
VALUES 
(@UserId, @MediaInception, 9.5, N'A mind-bending masterwork with Hans Zimmer''s iconic score. The dream layers concept is executed flawlessly.'),
(@UserId, @MediaDarkKnight, 10.0, N'Heath Ledger delivers the greatest villain performance in cinema history. Gripping from the opening bank heist.'),
(@AdminId, @MediaBreakingBad, 9.8, N'Tense, immaculate character transformation. Television storytelling at its absolute zenith.');

-- D. Sample Seed Watchlist
INSERT INTO dbo.Watchlist (UserId, MediaId, Status)
VALUES 
(@UserId, (SELECT MediaId FROM dbo.MediaItems WHERE ImdbId = N'tt0816692'), N'PlanToWatch'),
(@UserId, (SELECT MediaId FROM dbo.MediaItems WHERE ImdbId = N'tt4574334'), N'Watching');
GO

PRINT '=======================================================';
PRINT 'RateMeDb setup complete! Seed data and schema ready.';
PRINT 'Default Admin: admin / Admin@123';
PRINT 'Default User:  cinema_fan / User@123';
PRINT '=======================================================';
