using System;
using System.Security.Cryptography;
using System.Text;
using RateMe.Models;

namespace RateMe.Services
{
    public class AuthService
    {
        private readonly DatabaseService _db;
        private User _currentUser;

        public User CurrentUser => _currentUser;
        public bool IsLoggedIn => _currentUser != null;
        public bool IsAdmin => _currentUser != null && _currentUser.IsAdmin;

        public event Action<User> AuthStateChanged;

        public AuthService(DatabaseService db)
        {
            _db = db;
        }

        public bool Login(string usernameOrEmail, string password, out string errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Please enter both username and password.";
                return false;
            }

            var user = _db.GetUserByUsernameOrEmail(usernameOrEmail);
            if (user == null)
            {
                errorMessage = "No account found with this username or email.";
                return false;
            }

            if (!user.IsActive)
            {
                errorMessage = "This account is inactive. Please contact an administrator.";
                return false;
            }

            string computedHash = HashPassword(password, user.Salt);
            if (!string.Equals(computedHash, user.PasswordHash, StringComparison.Ordinal))
            {
                errorMessage = "Invalid password. Please check your credentials.";
                return false;
            }

            _currentUser = user;
            _db.UpdateLastLogin(user.UserId);
            AuthStateChanged?.Invoke(_currentUser);
            return true;
        }

        public bool Register(string username, string email, string password, string displayName, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            {
                errorMessage = "Username must be at least 3 characters long.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
            {
                errorMessage = "Please enter a valid email address.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                errorMessage = "Password must be at least 6 characters long.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = username;
            }

            if (_db.GetUserByUsernameOrEmail(username) != null)
            {
                errorMessage = "Username is already taken.";
                return false;
            }

            if (_db.GetUserByUsernameOrEmail(email) != null)
            {
                errorMessage = "Email is already registered.";
                return false;
            }

            string salt = GenerateSalt();
            string hash = HashPassword(password, salt);

            var newUser = new User
            {
                Username = username.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = hash,
                Salt = salt,
                DisplayName = displayName.Trim(),
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            if (_db.CreateUser(newUser))
            {
                _currentUser = _db.GetUserByUsernameOrEmail(username);
                AuthStateChanged?.Invoke(_currentUser);
                return true;
            }

            errorMessage = "Failed to create user in database.";
            return false;
        }

        public void Logout()
        {
            _currentUser = null;
            AuthStateChanged?.Invoke(null);
        }

        public bool UpdateProfile(string displayName, string bio, out string errorMessage)
        {
            errorMessage = null;
            if (_currentUser == null)
            {
                errorMessage = "Not logged in.";
                return false;
            }

            _currentUser.DisplayName = displayName;
            _currentUser.Bio = bio;
            _db.UpdateUserProfile(_currentUser);
            AuthStateChanged?.Invoke(_currentUser);
            return true;
        }

        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        public static string HashPassword(string password, string salt)
        {
            using (var sha = SHA256.Create())
            {
                byte[] combined = Encoding.UTF8.GetBytes(password + ":" + salt);
                byte[] hash = sha.ComputeHash(combined);
                var sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}

