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
),
(
    N'tt0110912',
    N'Pulp Fiction',
    N'1994',
    N'movie',
    N'Crime, Drama',
    N'Quentin Tarantino',
    N'John Travolta, Uma Thurman, Samuel L. Jackson',
    N'The lives of two mob hitmen, a boxer, a gangster and his wife, and a pair of diner bandits intertwine in four tales of violence and redemption.',
    N'https://m.media-amazon.com/images/M/MV5BYTViYTE3ZGQtNDBlMC00ZTAyLTkyODMtZGRiZDg0MjA2YThkXkEyXkFqcGc@._V1_QL75_UY562_CR3,0,380,562_.jpg',
    8.8,
    95,
    N'154 min'
),
(
    N'tt0944947',
    N'Game of Thrones',
    N'2011–2019',
    N'series',
    N'Drama, Fantasy, Action',
    N'David Benioff, D.B. Weiss',
    N'Emilia Clarke, Peter Dinklage, Kit Harington',
    N'Nine noble families fight for control over the lands of Westeros, while an ancient enemy returns after being dormant for millennia.',
    N'https://m.media-amazon.com/images/M/MV5BNGYxOGJkMjItZjVkZC00OGEzLWExNjktOTZmNGZhZmRlMTk2XkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    9.2,
    86,
    N'57 min'
),
(
    N'tt0137523',
    N'Fight Club',
    N'1999',
    N'movie',
    N'Crime, Drama, Thriller',
    N'David Fincher',
    N'Brad Pitt, Edward Norton, Meat Loaf',
    N'An insomniac office worker and a devil-may-care soap maker form an underground fight club that evolves into much more.',
    N'https://m.media-amazon.com/images/M/MV5BOTgyOGQ1NDItNGU3Ny00MjU3LTg2YWEtNmEyYjBiMjI1Y2M5XkEyXkFqcGc@._V1_QL75_UX380_CR0,4,380,562_.jpg',
    8.8,
    67,
    N'139 min'
),
(
    N'tt0109830',
    N'Forrest Gump',
    N'1994',
    N'movie',
    N'Drama, Romance',
    N'Robert Zemeckis',
    N'Tom Hanks, Robin Wright, Gary Sinise',
    N'The history of the United States from the 1950s to the 70s unfolds from the perspective of an Alabama man with an IQ of 75, who yearns to be reunited with his childhood sweetheart.',
    N'https://m.media-amazon.com/images/M/MV5BNDYwNzVjMTItZmU5YS00YjQ5LTljYjgtMjY2NDVmYWMyNWFmXkEyXkFqcGc@._V1_QL75_UY562_CR4,0,380,562_.jpg',
    8.8,
    82,
    N'142 min'
),
(
    N'tt0068646',
    N'The Godfather',
    N'1972',
    N'movie',
    N'Crime, Drama',
    N'Francis Ford Coppola',
    N'Marlon Brando, Al Pacino, James Caan',
    N'The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant son.',
    N'https://m.media-amazon.com/images/M/MV5BNGEwYjgwOGQtYjg5ZS00Njc1LTk2ZGEtM2QwZWQ2NjdhZTE5XkEyXkFqcGc@._V1_QL75_UY562_CR8,0,380,562_.jpg',
    9.2,
    100,
    N'175 min'
),
(
    N'tt8770514',
    N'Chernobyl',
    N'2019',
    N'series',
    N'Drama, History, Thriller',
    N'Craig Mazin',
    N'Jessie Buckley, Jared Harris, Stellan Skarsgård',
    N'In April 1986, an explosion at the Chernobyl nuclear power plant in the Union of Soviet Socialist Republics becomes one of the world''s worst man-made catastrophes.',
    N'https://m.media-amazon.com/images/M/MV5BNTEzNjBiMTAtM2NiYi00N2Q4LTljMjgtZGMxNTZmY2IxNzE4XkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    9.3,
    82,
    N'330 min'
),
(
    N'tt0120737',
    N'The Lord of the Rings: The Fellowship of the Ring',
    N'2001',
    N'movie',
    N'Action, Adventure, Drama',
    N'Peter Jackson',
    N'Elijah Wood, Ian McKellen, Orlando Bloom',
    N'A meek Hobbit from the Shire and eight companions set out on a journey to destroy the powerful One Ring and save Middle-earth from the Dark Lord Sauron.',
    N'https://m.media-amazon.com/images/M/MV5BNzIxMDQ2YTctNDY4MC00ZTRhLTk4ODQtMTVlOWY4NTdiYmMwXkEyXkFqcGc@._V1_QL75_UX380_CR0,1,380,562_.jpg',
    8.9,
    92,
    N'178 min'
),
(
    N'tt3032476',
    N'Better Call Saul',
    N'2015–2022',
    N'series',
    N'Crime, Drama',
    N'Vince Gilligan, Peter Gould',
    N'Bob Odenkirk, Rhea Seehorn, Jonathan Banks',
    N'The trials and tribulations of criminal lawyer Jimmy McGill in the years leading up to his fateful run-in with Walter White and Jesse Pinkman.',
    N'https://m.media-amazon.com/images/M/MV5BNDJhYTk2ZTItZTI3Yi00OWQ1LWFkNmUtMGU3NTRjOGE4MGQ0XkEyXkFqcGc@._V1_QL75_UX380_CR0,4,380,562_.jpg',
    9.0,
    86,
    N'46 min'
),
(
    N'tt15239678',
    N'Dune: Part Two',
    N'2024',
    N'movie',
    N'Action, Adventure, Drama',
    N'Denis Villeneuve',
    N'Timothée Chalamet, Zendaya, Rebecca Ferguson',
    N'Paul Atreides unites with Chani and the Fremen while seeking revenge against the conspirators who destroyed his family.',
    N'https://m.media-amazon.com/images/M/MV5BNTc0YmQxMjEtODI5MC00NjFiLTlkMWUtOGQ5NjFmYWUyZGJhXkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    8.5,
    79,
    N'166 min'
),
(
    N'tt0141842',
    N'The Sopranos',
    N'1999–2007',
    N'series',
    N'Crime, Drama',
    N'David Chase',
    N'James Gandolfini, Lorraine Bracco, Edie Falco',
    N'New Jersey mob boss Tony Soprano deals with personal and professional issues in his home and business life that''s affecting his mental state.',
    N'https://m.media-amazon.com/images/M/MV5BNmQ4YmE0M2UtNDk5Yi00MzFhLWEwZmMtNzg4YmE5ZGU5Y2E3XkEyXkFqcGc@._V1_QL75_UX380_CR0,4,380,562_.jpg',
    9.2,
    97,
    N'55 min'
),
(
    N'tt0172495',
    N'Gladiator',
    N'2000',
    N'movie',
    N'Action, Adventure, Drama',
    N'Ridley Scott',
    N'Russell Crowe, Joaquin Phoenix, Connie Nielsen',
    N'A former Roman General sets out to exact vengeance against the corrupt emperor who murdered his family and sent him into slavery.',
    N'https://m.media-amazon.com/images/M/MV5BYWQ4YmNjYjEtOWE1Zi00Y2U4LWI4NTAtMTU0MjkxNWQ1ZmJiXkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    8.5,
    67,
    N'155 min'
),
(
    N'tt11280740',
    N'Severance',
    N'2022–',
    N'series',
    N'Drama, Mystery, Sci-Fi',
    N'Dan Erickson',
    N'Adam Scott, Zach Cherry, Britt Lower',
    N'Mark leads a team of office workers whose memories have been surgically divided between their work and personal lives.',
    N'https://m.media-amazon.com/images/M/MV5BMjkwZjcwMGQtNDAzOC00NWVkLTk1MTktNDlhNTBkZGYwN2RkXkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    8.7,
    83,
    N'50 min'
),
(
    N'tt0245429',
    N'Spirited Away',
    N'2001',
    N'movie',
    N'Animation, Adventure, Family',
    N'Hayao Miyazaki',
    N'Daveigh Chase, Suzanne Pleshette, Miyu Irino',
    N'During her family''s move to the suburbs, a sullen 10-year-old girl wanders into a world ruled by gods, witches, and spirits, a world where humans are changed into beasts.',
    N'https://m.media-amazon.com/images/M/MV5BNTEyNmEwOWUtYzkyOC00ZTQ4LTllZmUtMjk0Y2YwOGUzYjRiXkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    8.6,
    96,
    N'125 min'
),
(
    N'tt7660850',
    N'Succession',
    N'2018–2023',
    N'series',
    N'Drama',
    N'Jesse Armstrong',
    N'Nicholas Braun, Brian Cox, Kieran Culkin',
    N'The Roy family is known for controlling the biggest media and entertainment company in the world. However, their world changes when their father steps down from the company.',
    N'https://m.media-amazon.com/images/M/MV5BYTY4YTVkY2QtMjRmOS00YzliLWIxOWQtMTdkOTVkN2UzODNmXkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    8.9,
    89,
    N'60 min'
),
(
    N'tt3581920',
    N'The Last of Us',
    N'2023–',
    N'series',
    N'Action, Adventure, Drama',
    N'Neil Druckmann, Craig Mazin',
    N'Pedro Pascal, Bella Ramsey, Gabriel Luna',
    N'After a global pandemic destroys civilization, a hardened survivor takes charge of a 14-year-old girl who may be humanity''s last hope.',
    N'https://m.media-amazon.com/images/M/MV5BYWI3ODJlMzktY2U5NC00ZjdlLWE1MGItNWQxZDk3NWNjN2RhXkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg',
    8.7,
    84,
    N'55 min'
),
(
    N'tt9362722',
    N'Spider-Man: Across the Spider-Verse',
    N'2023',
    N'movie',
    N'Animation, Action, Adventure',
    N'Joaquim Dos Santos, Kemp Powers, Justin K. Thompson',
    N'Shameik Moore, Hailee Steinfeld, Brian Tyree Henry',
    N'Miles Morales catapults across the Multiverse, where he encounters a team of Spider-People charged with protecting its very existence.',
    N'https://m.media-amazon.com/images/M/MV5BNThiZjA3MjItZGY5Ni00ZmJhLWEwN2EtOTBlYTA4Y2E0M2ZmXkEyXkFqcGc@._V1_QL75_UX380_CR0,1,380,562_.jpg',
    8.6,
    86,
    N'140 min'
),
(
    N'tt2442560',
    N'Peaky Blinders',
    N'2013–2022',
    N'series',
    N'Crime, Drama',
    N'Steven Knight',
    N'Cillian Murphy, Paul Anderson, Sophie Rundle',
    N'A gangster family epic set in 1919 Birmingham, England; centered on a gang who sew razor blades in the peaks of their caps, and their fierce boss Tommy Shelby.',
    N'https://m.media-amazon.com/images/M/MV5BOGM0NGY3ZmItOGE2ZC00OWIxLTk0N2EtZWY4Yzg3ZDlhNGI3XkEyXkFqcGc@._V1_QL75_UX380_CR0,4,380,562_.jpg',
    8.8,
    77,
    N'60 min'
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
