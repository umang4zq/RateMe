using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using RateMe.Models;

namespace RateMe.Services
{
    public class DatabaseService
    {
        private string _connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=RateMeDb;Trusted_Connection=True;";
        private bool _isUsingSqlServer = false;
        private static readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private readonly string _localStoragePath;

        // In-memory fallback structures
        private readonly List<User> _localUsers = new List<User>();
        private readonly List<MediaItem> _localMedia = new List<MediaItem>();
        private readonly List<Rating> _localRatings = new List<Rating>();
        private readonly List<WatchlistItem> _localWatchlist = new List<WatchlistItem>();

        public bool IsUsingSqlServer => _isUsingSqlServer;
        public string ConnectionString
        {
            get => _connectionString;
            set => _connectionString = value;
        }

        public DatabaseService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string rateMeDir = Path.Combine(appData, "RateMe");
            if (!Directory.Exists(rateMeDir))
            {
                Directory.CreateDirectory(rateMeDir);
            }
            _localStoragePath = Path.Combine(rateMeDir, "rateme_storage.json");

            Initialize();
        }

        public void Initialize()
        {
            _isUsingSqlServer = TryConnectSqlServer();

            if (_isUsingSqlServer)
            {
                EnsureSqlServerTablesExist();
            }
            else
            {
                LoadLocalBackup();
            }
        }

        public bool TryConnectSqlServer(string testConnStr = null)
        {
            string connToTest = testConnStr ?? _connectionString;
            try
            {
                using (var conn = new SqlConnection(connToTest))
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        private void EnsureSqlServerTablesExist()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string checkSql = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                        BEGIN
                            CREATE TABLE dbo.Users (
                                UserId INT IDENTITY(1,1) PRIMARY KEY,
                                Username NVARCHAR(50) UNIQUE NOT NULL,
                                Email NVARCHAR(100) UNIQUE NOT NULL,
                                PasswordHash NVARCHAR(256) NOT NULL,
                                Salt NVARCHAR(64) NOT NULL,
                                DisplayName NVARCHAR(100) NOT NULL,
                                Bio NVARCHAR(500) NULL,
                                AvatarUrl NVARCHAR(500) NULL,
                                Role NVARCHAR(20) DEFAULT 'User',
                                IsActive BIT DEFAULT 1,
                                CreatedAt DATETIME DEFAULT GETDATE(),
                                LastLoginAt DATETIME NULL
                            );
                        END;

                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MediaItems')
                        BEGIN
                            CREATE TABLE dbo.MediaItems (
                                MediaId INT IDENTITY(1,1) PRIMARY KEY,
                                ImdbId NVARCHAR(30) UNIQUE NOT NULL,
                                Title NVARCHAR(255) NOT NULL,
                                Year NVARCHAR(20) NULL,
                                Type NVARCHAR(20) DEFAULT 'movie',
                                Genre NVARCHAR(200) NULL,
                                Director NVARCHAR(200) NULL,
                                Actors NVARCHAR(500) NULL,
                                Plot NVARCHAR(MAX) NULL,
                                PosterUrl NVARCHAR(1000) NULL,
                                ImdbRating DECIMAL(3,1) NULL,
                                Metascore INT NULL,
                                Runtime NVARCHAR(50) NULL,
                                TotalUserRatings INT DEFAULT 0,
                                AverageUserRating DECIMAL(3,2) DEFAULT 0.0,
                                CreatedAt DATETIME DEFAULT GETDATE(),
                                UpdatedAt DATETIME DEFAULT GETDATE()
                            );
                        END;

                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Ratings')
                        BEGIN
                            CREATE TABLE dbo.Ratings (
                                RatingId INT IDENTITY(1,1) PRIMARY KEY,
                                UserId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
                                MediaId INT NOT NULL FOREIGN KEY REFERENCES dbo.MediaItems(MediaId) ON DELETE CASCADE,
                                Score DECIMAL(3,1) NOT NULL,
                                ReviewText NVARCHAR(MAX) NULL,
                                CreatedAt DATETIME DEFAULT GETDATE(),
                                UpdatedAt DATETIME DEFAULT GETDATE(),
                                CONSTRAINT UQ_Ratings_User_Media UNIQUE (UserId, MediaId)
                            );
                        END;

                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Watchlist')
                        BEGIN
                            CREATE TABLE dbo.Watchlist (
                                WatchlistId INT IDENTITY(1,1) PRIMARY KEY,
                                UserId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
                                MediaId INT NOT NULL FOREIGN KEY REFERENCES dbo.MediaItems(MediaId) ON DELETE CASCADE,
                                Status NVARCHAR(20) DEFAULT 'PlanToWatch',
                                AddedAt DATETIME DEFAULT GETDATE(),
                                CONSTRAINT UQ_Watchlist_User_Media UNIQUE (UserId, MediaId)
                            );
                        END;
                    ";

                    using (var cmd = new SqlCommand(checkSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error ensuring SQL tables: " + ex.Message);
                _isUsingSqlServer = false;
                LoadLocalBackup();
            }
        }

        #region Local JSON Store Fallback

        private void LoadLocalBackup()
        {
            if (File.Exists(_localStoragePath))
            {
                try
                {
                    string json = File.ReadAllText(_localStoragePath);
                    var state = _serializer.Deserialize<LocalStateStore>(json);
                    if (state != null)
                    {
                        _localUsers.Clear();
                        _localUsers.AddRange(state.Users ?? new List<User>());
                        _localMedia.Clear();
                        _localMedia.AddRange(state.MediaItems ?? new List<MediaItem>());
                        _localRatings.Clear();
                        _localRatings.AddRange(state.Ratings ?? new List<Rating>());
                        _localWatchlist.Clear();
                        _localWatchlist.AddRange(state.Watchlist ?? new List<WatchlistItem>());
                    }
                }
                catch
                {
                    SeedLocalDefaults();
                }
            }

            if (_localUsers.Count == 0 || _localMedia.Count == 0)
            {
                SeedLocalDefaults();
            }
        }

        private void SeedLocalDefaults()
        {
            _localUsers.Clear();
            string adminSalt = AuthService.GenerateSalt();
            string userSalt = AuthService.GenerateSalt();

            _localUsers.Add(new User
            {
                UserId = 1,
                Username = "admin",
                Email = "admin@rateme.app",
                PasswordHash = AuthService.HashPassword("Admin@123", adminSalt),
                Salt = adminSalt,
                DisplayName = "RateMe Director",
                Bio = "Chief Curator and Administrator at RateMe Studio.",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            _localUsers.Add(new User
            {
                UserId = 2,
                Username = "cinema_fan",
                Email = "fan@rateme.app",
                PasswordHash = AuthService.HashPassword("User@123", userSalt),
                Salt = userSalt,
                DisplayName = "Aria Sterling",
                Bio = "Film buff, coffee lover, and weekend series binger.",
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            _localMedia.Clear();
            int id = 1;
            foreach (var item in SeedData.GetCuratedMedia())
            {
                item.MediaId = id++;
                _localMedia.Add(item);
            }

            _localRatings.Clear();
            _localRatings.Add(new Rating
            {
                RatingId = 1,
                UserId = 2,
                MediaId = 1,
                Score = 9.5m,
                ReviewText = "A mind-bending masterwork with Hans Zimmer's iconic score. The dream layers concept is executed flawlessly.",
                CreatedAt = DateTime.Now.AddDays(-3),
                Username = "cinema_fan",
                UserDisplayName = "Aria Sterling",
                MediaTitle = "Inception",
                MediaImdbId = "tt1375666",
                MediaPosterUrl = "https://m.media-amazon.com/images/M/MV5BMjAxMzY3NjcxNF5BMl5BanBnXkFtZTcwNTI5OTM0Mw@@._V1_QL75_UX380_CR0,0,380,562_.jpg"
            });

            _localRatings.Add(new Rating
            {
                RatingId = 2,
                UserId = 2,
                MediaId = 2,
                Score = 10.0m,
                ReviewText = "Heath Ledger delivers the greatest villain performance in cinema history. Gripping from the opening bank heist.",
                CreatedAt = DateTime.Now.AddDays(-1),
                Username = "cinema_fan",
                UserDisplayName = "Aria Sterling",
                MediaTitle = "The Dark Knight",
                MediaImdbId = "tt0468569",
                MediaPosterUrl = "https://m.media-amazon.com/images/M/MV5BMTMxNTMwODM0NF5BMl5BanBnXkFtZTcwODAyMTk2Mw@@._V1_QL75_UX380_CR0,0,380,562_.jpg"
            });

            _localWatchlist.Clear();
            _localWatchlist.Add(new WatchlistItem
            {
                WatchlistId = 1,
                UserId = 2,
                MediaId = 3,
                Status = "PlanToWatch",
                AddedAt = DateTime.Now.AddDays(-2),
                Media = _localMedia.FirstOrDefault(m => m.MediaId == 3)
            });

            SaveLocalBackup();
        }

        private void SaveLocalBackup()
        {
            try
            {
                var state = new LocalStateStore
                {
                    Users = _localUsers,
                    MediaItems = _localMedia,
                    Ratings = _localRatings,
                    Watchlist = _localWatchlist
                };
                string json = _serializer.Serialize(state);
                File.WriteAllText(_localStoragePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error saving local store: " + ex.Message);
            }
        }

        private class LocalStateStore
        {
            public List<User> Users { get; set; }
            public List<MediaItem> MediaItems { get; set; }
            public List<Rating> Ratings { get; set; }
            public List<WatchlistItem> Watchlist { get; set; }
        }

        #endregion

        #region User Operations (CRUD)

        public User GetUserByUsernameOrEmail(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return null;

            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "SELECT TOP 1 * FROM dbo.Users WHERE Username = @id OR Email = @id";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", identifier);
                            using (var r = cmd.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    return ReadUser(r);
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            return _localUsers.FirstOrDefault(u =>
                string.Equals(u.Username, identifier, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(u.Email, identifier, StringComparison.OrdinalIgnoreCase));
        }

        public bool CreateUser(User user)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            INSERT INTO dbo.Users (Username, Email, PasswordHash, Salt, DisplayName, Bio, Role, IsActive, CreatedAt)
                            VALUES (@u, @e, @p, @s, @d, @b, @r, @a, @c);
                            SELECT SCOPE_IDENTITY();
                        ";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@u", user.Username);
                            cmd.Parameters.AddWithValue("@e", user.Email);
                            cmd.Parameters.AddWithValue("@p", user.PasswordHash);
                            cmd.Parameters.AddWithValue("@s", user.Salt);
                            cmd.Parameters.AddWithValue("@d", user.DisplayName ?? user.Username);
                            cmd.Parameters.AddWithValue("@b", (object)user.Bio ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@r", user.Role ?? "User");
                            cmd.Parameters.AddWithValue("@a", user.IsActive);
                            cmd.Parameters.AddWithValue("@c", user.CreatedAt);

                            object res = cmd.ExecuteScalar();
                            user.UserId = Convert.ToInt32(res);
                            return true;
                        }
                    }
                }
                catch { }
            }

            user.UserId = _localUsers.Count > 0 ? _localUsers.Max(u => u.UserId) + 1 : 1;
            _localUsers.Add(user);
            SaveLocalBackup();
            return true;
        }

        public void UpdateUserProfile(User user)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "UPDATE dbo.Users SET DisplayName = @d, Bio = @b WHERE UserId = @id";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@d", user.DisplayName);
                            cmd.Parameters.AddWithValue("@b", (object)user.Bio ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@id", user.UserId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
            }

            var local = _localUsers.FirstOrDefault(u => u.UserId == user.UserId);
            if (local != null)
            {
                local.DisplayName = user.DisplayName;
                local.Bio = user.Bio;
                SaveLocalBackup();
            }
        }

        public bool UpdateUserAdmin(User user)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            UPDATE dbo.Users 
                            SET DisplayName = @d, Email = @e, Bio = @b, Role = @r, IsActive = @a
                            WHERE UserId = @id";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@d", user.DisplayName ?? user.Username);
                            cmd.Parameters.AddWithValue("@e", user.Email);
                            cmd.Parameters.AddWithValue("@b", (object)user.Bio ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@r", user.Role ?? "User");
                            cmd.Parameters.AddWithValue("@a", user.IsActive);
                            cmd.Parameters.AddWithValue("@id", user.UserId);
                            cmd.ExecuteNonQuery();
                            return true;
                        }
                    }
                }
                catch { }
            }

            var local = _localUsers.FirstOrDefault(u => u.UserId == user.UserId);
            if (local != null)
            {
                local.DisplayName = user.DisplayName;
                local.Email = user.Email;
                local.Bio = user.Bio;
                local.Role = user.Role;
                local.IsActive = user.IsActive;
                SaveLocalBackup();
                return true;
            }
            return false;
        }

        public bool DeleteUser(int userId)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM dbo.Users WHERE UserId = @id";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", userId);
                            cmd.ExecuteNonQuery();
                            return true;
                        }
                    }
                }
                catch { }
            }

            _localRatings.RemoveAll(r => r.UserId == userId);
            _localWatchlist.RemoveAll(w => w.UserId == userId);
            int removed = _localUsers.RemoveAll(u => u.UserId == userId);
            if (removed > 0)
            {
                SaveLocalBackup();
                return true;
            }
            return false;
        }

        public bool ResetUserPassword(int userId, string newPassword)
        {
            string salt = AuthService.GenerateSalt();
            string hash = AuthService.HashPassword(newPassword, salt);

            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "UPDATE dbo.Users SET PasswordHash = @p, Salt = @s WHERE UserId = @id";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@p", hash);
                            cmd.Parameters.AddWithValue("@s", salt);
                            cmd.Parameters.AddWithValue("@id", userId);
                            cmd.ExecuteNonQuery();
                            return true;
                        }
                    }
                }
                catch { }
            }

            var local = _localUsers.FirstOrDefault(u => u.UserId == userId);
            if (local != null)
            {
                local.PasswordHash = hash;
                local.Salt = salt;
                SaveLocalBackup();
                return true;
            }
            return false;
        }

        public void UpdateLastLogin(int userId)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "UPDATE dbo.Users SET LastLoginAt = GETDATE() WHERE UserId = @id";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", userId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
            }

            var local = _localUsers.FirstOrDefault(u => u.UserId == userId);
            if (local != null)
            {
                local.LastLoginAt = DateTime.Now;
                SaveLocalBackup();
            }
        }

        public List<User> GetAllUsers()
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    var list = new List<User>();
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "SELECT * FROM dbo.Users ORDER BY CreatedAt DESC";
                        using (var cmd = new SqlCommand(sql, conn))
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add(ReadUser(r));
                            }
                        }
                    }
                    if (list.Count > 0) return list;
                }
                catch { }
            }

            return new List<User>(_localUsers);
        }

        private User ReadUser(SqlDataReader r)
        {
            return new User
            {
                UserId = r.GetInt32(r.GetOrdinal("UserId")),
                Username = r.GetString(r.GetOrdinal("Username")),
                Email = r.GetString(r.GetOrdinal("Email")),
                PasswordHash = r.GetString(r.GetOrdinal("PasswordHash")),
                Salt = r.GetString(r.GetOrdinal("Salt")),
                DisplayName = r.GetString(r.GetOrdinal("DisplayName")),
                Bio = r.IsDBNull(r.GetOrdinal("Bio")) ? null : r.GetString(r.GetOrdinal("Bio")),
                AvatarUrl = r.IsDBNull(r.GetOrdinal("AvatarUrl")) ? null : r.GetString(r.GetOrdinal("AvatarUrl")),
                Role = r.GetString(r.GetOrdinal("Role")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive")),
                CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
                LastLoginAt = r.IsDBNull(r.GetOrdinal("LastLoginAt")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("LastLoginAt"))
            };
        }

        #endregion

        #region Media Operations (CRUD)

        public MediaItem GetOrCreateMediaItem(MediaItem media)
        {
            if (media == null || string.IsNullOrWhiteSpace(media.ImdbId)) return media;

            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string findSql = "SELECT * FROM dbo.MediaItems WHERE ImdbId = @imdb";
                        using (var cmd = new SqlCommand(findSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@imdb", media.ImdbId);
                            using (var r = cmd.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    return ReadMediaItem(r);
                                }
                            }
                        }

                        string insertSql = @"
                            INSERT INTO dbo.MediaItems (ImdbId, Title, Year, Type, Genre, Director, Actors, Plot, PosterUrl, ImdbRating, Metascore, Runtime)
                            VALUES (@imdb, @t, @y, @type, @g, @dir, @act, @p, @post, @ir, @ms, @rt);
                            SELECT SCOPE_IDENTITY();
                        ";
                        using (var cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@imdb", media.ImdbId);
                            cmd.Parameters.AddWithValue("@t", media.Title);
                            cmd.Parameters.AddWithValue("@y", (object)media.Year ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@type", media.Type ?? "movie");
                            cmd.Parameters.AddWithValue("@g", (object)media.Genre ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@dir", (object)media.Director ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@act", (object)media.Actors ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p", (object)media.Plot ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@post", (object)media.PosterUrl ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ir", (object)media.ImdbRating ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ms", (object)media.Metascore ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@rt", (object)media.Runtime ?? DBNull.Value);

                            media.MediaId = Convert.ToInt32(cmd.ExecuteScalar());
                            return media;
                        }
                    }
                }
                catch { }
            }

            var existing = _localMedia.FirstOrDefault(m => string.Equals(m.ImdbId, media.ImdbId, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return existing;
            }

            media.MediaId = _localMedia.Count > 0 ? _localMedia.Max(m => m.MediaId) + 1 : 1;
            _localMedia.Add(media);
            SaveLocalBackup();
            return media;
        }

        public bool DeleteMediaItem(int mediaId)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM dbo.MediaItems WHERE MediaId = @mid";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@mid", mediaId);
                            cmd.ExecuteNonQuery();
                            return true;
                        }
                    }
                }
                catch { }
            }

            _localRatings.RemoveAll(r => r.MediaId == mediaId);
            _localWatchlist.RemoveAll(w => w.MediaId == mediaId);
            int removed = _localMedia.RemoveAll(m => m.MediaId == mediaId);
            if (removed > 0)
            {
                SaveLocalBackup();
                return true;
            }
            return false;
        }

        public List<MediaItem> GetCuratedMedia()
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    var list = new List<MediaItem>();
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "SELECT * FROM dbo.MediaItems ORDER BY ImdbRating DESC";
                        using (var cmd = new SqlCommand(sql, conn))
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add(ReadMediaItem(r));
                            }
                        }
                    }
                    if (list.Count > 0) return list;
                }
                catch { }
            }

            return new List<MediaItem>(_localMedia);
        }

        private MediaItem ReadMediaItem(SqlDataReader r)
        {
            return new MediaItem
            {
                MediaId = r.GetInt32(r.GetOrdinal("MediaId")),
                ImdbId = r.GetString(r.GetOrdinal("ImdbId")),
                Title = r.GetString(r.GetOrdinal("Title")),
                Year = r.IsDBNull(r.GetOrdinal("Year")) ? null : r.GetString(r.GetOrdinal("Year")),
                Type = r.GetString(r.GetOrdinal("Type")),
                Genre = r.IsDBNull(r.GetOrdinal("Genre")) ? null : r.GetString(r.GetOrdinal("Genre")),
                Director = r.IsDBNull(r.GetOrdinal("Director")) ? null : r.GetString(r.GetOrdinal("Director")),
                Actors = r.IsDBNull(r.GetOrdinal("Actors")) ? null : r.GetString(r.GetOrdinal("Actors")),
                Plot = r.IsDBNull(r.GetOrdinal("Plot")) ? null : r.GetString(r.GetOrdinal("Plot")),
                PosterUrl = r.IsDBNull(r.GetOrdinal("PosterUrl")) ? null : r.GetString(r.GetOrdinal("PosterUrl")),
                ImdbRating = r.IsDBNull(r.GetOrdinal("ImdbRating")) ? (decimal?)null : r.GetDecimal(r.GetOrdinal("ImdbRating")),
                Metascore = r.IsDBNull(r.GetOrdinal("Metascore")) ? (int?)null : r.GetInt32(r.GetOrdinal("Metascore")),
                Runtime = r.IsDBNull(r.GetOrdinal("Runtime")) ? null : r.GetString(r.GetOrdinal("Runtime")),
                TotalUserRatings = r.GetInt32(r.GetOrdinal("TotalUserRatings")),
                AverageUserRating = r.GetDecimal(r.GetOrdinal("AverageUserRating")),
                CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
                UpdatedAt = r.GetDateTime(r.GetOrdinal("UpdatedAt"))
            };
        }

        #endregion

        #region Rating Operations & Moderation

        public bool SaveRating(int userId, int mediaId, decimal score, string review)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            MERGE dbo.Ratings AS target
                            USING (SELECT @uid AS UserId, @mid AS MediaId) AS source
                            ON (target.UserId = source.UserId AND target.MediaId = source.MediaId)
                            WHEN MATCHED THEN
                                UPDATE SET Score = @score, ReviewText = @rev, UpdatedAt = GETDATE()
                            WHEN NOT MATCHED THEN
                                INSERT (UserId, MediaId, Score, ReviewText, CreatedAt, UpdatedAt)
                                VALUES (@uid, @mid, @score, @rev, GETDATE(), GETDATE());
                        ";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@uid", userId);
                            cmd.Parameters.AddWithValue("@mid", mediaId);
                            cmd.Parameters.AddWithValue("@score", score);
                            cmd.Parameters.AddWithValue("@rev", (object)review ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                            return true;
                        }
                    }
                }
                catch { }
            }

            var existing = _localRatings.FirstOrDefault(r => r.UserId == userId && r.MediaId == mediaId);
            var user = _localUsers.FirstOrDefault(u => u.UserId == userId);
            var media = _localMedia.FirstOrDefault(m => m.MediaId == mediaId);

            if (existing != null)
            {
                existing.Score = score;
                existing.ReviewText = review;
                existing.UpdatedAt = DateTime.Now;
            }
            else
            {
                _localRatings.Add(new Rating
                {
                    RatingId = _localRatings.Count > 0 ? _localRatings.Max(r => r.RatingId) + 1 : 1,
                    UserId = userId,
                    MediaId = mediaId,
                    Score = score,
                    ReviewText = review,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Username = user?.Username,
                    UserDisplayName = user?.DisplayName,
                    MediaTitle = media?.Title,
                    MediaImdbId = media?.ImdbId,
                    MediaPosterUrl = media?.PosterUrl
                });
            }

            if (media != null)
            {
                var ratingsForMedia = _localRatings.Where(r => r.MediaId == mediaId).ToList();
                media.TotalUserRatings = ratingsForMedia.Count;
                media.AverageUserRating = ratingsForMedia.Count > 0 ? Math.Round(ratingsForMedia.Average(r => r.Score), 2) : 0m;
            }

            SaveLocalBackup();
            return true;
        }

        public bool DeleteRating(int ratingId)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM dbo.Ratings WHERE RatingId = @rid";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@rid", ratingId);
                            cmd.ExecuteNonQuery();
                            return true;
                        }
                    }
                }
                catch { }
            }

            int removed = _localRatings.RemoveAll(r => r.RatingId == ratingId);
            if (removed > 0)
            {
                SaveLocalBackup();
                return true;
            }
            return false;
        }

        public Rating GetUserRatingForMedia(int userId, int mediaId)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "SELECT * FROM dbo.Ratings WHERE UserId = @uid AND MediaId = @mid";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@uid", userId);
                            cmd.Parameters.AddWithValue("@mid", mediaId);
                            using (var r = cmd.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    return new Rating
                                    {
                                        RatingId = r.GetInt32(r.GetOrdinal("RatingId")),
                                        UserId = r.GetInt32(r.GetOrdinal("UserId")),
                                        MediaId = r.GetInt32(r.GetOrdinal("MediaId")),
                                        Score = r.GetDecimal(r.GetOrdinal("Score")),
                                        ReviewText = r.IsDBNull(r.GetOrdinal("ReviewText")) ? null : r.GetString(r.GetOrdinal("ReviewText")),
                                        CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
                                        UpdatedAt = r.GetDateTime(r.GetOrdinal("UpdatedAt"))
                                    };
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            return _localRatings.FirstOrDefault(r => r.UserId == userId && r.MediaId == mediaId);
        }

        public List<Rating> GetUserRatings(int userId)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    var list = new List<Rating>();
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            SELECT r.*, m.Title AS MediaTitle, m.ImdbId AS MediaImdbId, m.PosterUrl AS MediaPosterUrl, u.Username, u.DisplayName AS UserDisplayName
                            FROM dbo.Ratings r
                            JOIN dbo.MediaItems m ON r.MediaId = m.MediaId
                            JOIN dbo.Users u ON r.UserId = u.UserId
                            WHERE r.UserId = @uid
                            ORDER BY r.UpdatedAt DESC
                        ";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@uid", userId);
                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    list.Add(new Rating
                                    {
                                        RatingId = r.GetInt32(r.GetOrdinal("RatingId")),
                                        UserId = r.GetInt32(r.GetOrdinal("UserId")),
                                        MediaId = r.GetInt32(r.GetOrdinal("MediaId")),
                                        Score = r.GetDecimal(r.GetOrdinal("Score")),
                                        ReviewText = r.IsDBNull(r.GetOrdinal("ReviewText")) ? null : r.GetString(r.GetOrdinal("ReviewText")),
                                        CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
                                        UpdatedAt = r.GetDateTime(r.GetOrdinal("UpdatedAt")),
                                        MediaTitle = r.GetString(r.GetOrdinal("MediaTitle")),
                                        MediaImdbId = r.GetString(r.GetOrdinal("MediaImdbId")),
                                        MediaPosterUrl = r.IsDBNull(r.GetOrdinal("MediaPosterUrl")) ? null : r.GetString(r.GetOrdinal("MediaPosterUrl")),
                                        Username = r.GetString(r.GetOrdinal("Username")),
                                        UserDisplayName = r.GetString(r.GetOrdinal("UserDisplayName"))
                                    });
                                }
                            }
                        }
                    }
                    return list;
                }
                catch { }
            }

            var results = _localRatings.Where(r => r.UserId == userId).OrderByDescending(r => r.UpdatedAt).ToList();
            foreach (var r in results)
            {
                var media = _localMedia.FirstOrDefault(m => m.MediaId == r.MediaId);
                if (media != null)
                {
                    r.MediaTitle = media.Title;
                    r.MediaImdbId = media.ImdbId;
                    r.MediaPosterUrl = media.PosterUrl;
                }
            }
            return results;
        }

        public List<Rating> GetMediaRatings(int mediaId)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    var list = new List<Rating>();
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            SELECT r.*, u.Username, u.DisplayName AS UserDisplayName
                            FROM dbo.Ratings r
                            JOIN dbo.Users u ON r.UserId = u.UserId
                            WHERE r.MediaId = @mid
                            ORDER BY r.CreatedAt DESC
                        ";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@mid", mediaId);
                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    list.Add(new Rating
                                    {
                                        RatingId = r.GetInt32(r.GetOrdinal("RatingId")),
                                        UserId = r.GetInt32(r.GetOrdinal("UserId")),
                                        MediaId = r.GetInt32(r.GetOrdinal("MediaId")),
                                        Score = r.GetDecimal(r.GetOrdinal("Score")),
                                        ReviewText = r.IsDBNull(r.GetOrdinal("ReviewText")) ? null : r.GetString(r.GetOrdinal("ReviewText")),
                                        CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
                                        Username = r.GetString(r.GetOrdinal("Username")),
                                        UserDisplayName = r.GetString(r.GetOrdinal("UserDisplayName"))
                                    });
                                }
                            }
                        }
                    }
                    return list;
                }
                catch { }
            }

            return _localRatings.Where(r => r.MediaId == mediaId).OrderByDescending(r => r.CreatedAt).ToList();
        }

        public List<Rating> GetAllRatingsWithDetails()
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    var list = new List<Rating>();
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            SELECT r.*, m.Title AS MediaTitle, m.ImdbId AS MediaImdbId, m.PosterUrl AS MediaPosterUrl, u.Username, u.DisplayName AS UserDisplayName
                            FROM dbo.Ratings r
                            JOIN dbo.MediaItems m ON r.MediaId = m.MediaId
                            JOIN dbo.Users u ON r.UserId = u.UserId
                            ORDER BY r.CreatedAt DESC
                        ";
                        using (var cmd = new SqlCommand(sql, conn))
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add(new Rating
                                {
                                    RatingId = r.GetInt32(r.GetOrdinal("RatingId")),
                                    UserId = r.GetInt32(r.GetOrdinal("UserId")),
                                    MediaId = r.GetInt32(r.GetOrdinal("MediaId")),
                                    Score = r.GetDecimal(r.GetOrdinal("Score")),
                                    ReviewText = r.IsDBNull(r.GetOrdinal("ReviewText")) ? null : r.GetString(r.GetOrdinal("ReviewText")),
                                    CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
                                    UpdatedAt = r.GetDateTime(r.GetOrdinal("UpdatedAt")),
                                    MediaTitle = r.GetString(r.GetOrdinal("MediaTitle")),
                                    MediaImdbId = r.GetString(r.GetOrdinal("MediaImdbId")),
                                    MediaPosterUrl = r.IsDBNull(r.GetOrdinal("MediaPosterUrl")) ? null : r.GetString(r.GetOrdinal("MediaPosterUrl")),
                                    Username = r.GetString(r.GetOrdinal("Username")),
                                    UserDisplayName = r.GetString(r.GetOrdinal("UserDisplayName"))
                                });
                            }
                        }
                    }
                    if (list.Count > 0) return list;
                }
                catch { }
            }

            var results = new List<Rating>(_localRatings);
            foreach (var r in results)
            {
                var media = _localMedia.FirstOrDefault(m => m.MediaId == r.MediaId);
                var user = _localUsers.FirstOrDefault(u => u.UserId == r.UserId);
                if (media != null)
                {
                    r.MediaTitle = media.Title;
                    r.MediaImdbId = media.ImdbId;
                    r.MediaPosterUrl = media.PosterUrl;
                }
                if (user != null)
                {
                    r.Username = user.Username;
                    r.UserDisplayName = user.DisplayName;
                }
            }
            return results.OrderByDescending(r => r.CreatedAt).ToList();
        }

        #endregion

        #region Watchlist Operations

        public bool AddToWatchlist(int userId, int mediaId, string status = "PlanToWatch")
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            MERGE dbo.Watchlist AS target
                            USING (SELECT @uid AS UserId, @mid AS MediaId) AS source
                            ON (target.UserId = source.UserId AND target.MediaId = source.MediaId)
                            WHEN MATCHED THEN
                                UPDATE SET Status = @stat
                            WHEN NOT MATCHED THEN
                                INSERT (UserId, MediaId, Status, AddedAt)
                                VALUES (@uid, @mid, @stat, GETDATE());
                        ";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@uid", userId);
                            cmd.Parameters.AddWithValue("@mid", mediaId);
                            cmd.Parameters.AddWithValue("@stat", status);
                            cmd.ExecuteNonQuery();
                            return true;
                        }
                    }
                }
                catch { }
            }

            var item = _localWatchlist.FirstOrDefault(w => w.UserId == userId && w.MediaId == mediaId);
            if (item != null)
            {
                item.Status = status;
            }
            else
            {
                _localWatchlist.Add(new WatchlistItem
                {
                    WatchlistId = _localWatchlist.Count > 0 ? _localWatchlist.Max(w => w.WatchlistId) + 1 : 1,
                    UserId = userId,
                    MediaId = mediaId,
                    Status = status,
                    AddedAt = DateTime.Now,
                    Media = _localMedia.FirstOrDefault(m => m.MediaId == mediaId)
                });
            }

            SaveLocalBackup();
            return true;
        }

        public bool RemoveFromWatchlist(int userId, int mediaId)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM dbo.Watchlist WHERE UserId = @uid AND MediaId = @mid";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@uid", userId);
                            cmd.Parameters.AddWithValue("@mid", mediaId);
                            cmd.ExecuteNonQuery();
                            return true;
                        }
                    }
                }
                catch { }
            }

            int removed = _localWatchlist.RemoveAll(w => w.UserId == userId && w.MediaId == mediaId);
            if (removed > 0)
            {
                SaveLocalBackup();
                return true;
            }
            return false;
        }

        public bool IsInWatchlist(int userId, int mediaId)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = "SELECT COUNT(1) FROM dbo.Watchlist WHERE UserId = @uid AND MediaId = @mid";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@uid", userId);
                            cmd.Parameters.AddWithValue("@mid", mediaId);
                            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                        }
                    }
                }
                catch { }
            }

            return _localWatchlist.Any(w => w.UserId == userId && w.MediaId == mediaId);
        }

        public List<WatchlistItem> GetWatchlistForUser(int userId)
        {
            if (_isUsingSqlServer)
            {
                try
                {
                    var list = new List<WatchlistItem>();
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            SELECT w.*, m.*
                            FROM dbo.Watchlist w
                            JOIN dbo.MediaItems m ON w.MediaId = m.MediaId
                            WHERE w.UserId = @uid
                            ORDER BY w.AddedAt DESC
                        ";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@uid", userId);
                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    list.Add(new WatchlistItem
                                    {
                                        WatchlistId = r.GetInt32(r.GetOrdinal("WatchlistId")),
                                        UserId = r.GetInt32(r.GetOrdinal("UserId")),
                                        MediaId = r.GetInt32(r.GetOrdinal("MediaId")),
                                        Status = r.GetString(r.GetOrdinal("Status")),
                                        AddedAt = r.GetDateTime(r.GetOrdinal("AddedAt")),
                                        Media = ReadMediaItem(r)
                                    });
                                }
                            }
                        }
                    }
                    return list;
                }
                catch { }
            }

            var items = _localWatchlist.Where(w => w.UserId == userId).OrderByDescending(w => w.AddedAt).ToList();
            foreach (var item in items)
            {
                if (item.Media == null)
                {
                    item.Media = _localMedia.FirstOrDefault(m => m.MediaId == item.MediaId);
                }
            }
            return items;
        }

        #endregion

        #region Admin Metrics

        public AdminStats GetAdminStats()
        {
            var users = GetAllUsers();
            var media = GetCuratedMedia();
            var ratings = GetAllRatingsWithDetails();

            int watchlistCount = _localWatchlist.Count;
            if (_isUsingSqlServer)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.Watchlist", conn))
                        {
                            watchlistCount = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
                catch { }
            }

            return new AdminStats
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsActive),
                TotalMedia = media.Count,
                TotalRatings = ratings.Count,
                TotalWatchlist = watchlistCount,
                IsUsingSqlServer = _isUsingSqlServer
            };
        }

        #endregion
    }
}
