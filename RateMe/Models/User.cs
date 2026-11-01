using System;

namespace RateMe.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
        public string Role { get; set; } = "User"; // "User", "Admin"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }

        public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    public class AdminStats
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalMedia { get; set; }
        public int TotalRatings { get; set; }
        public int TotalWatchlist { get; set; }
        public bool IsUsingSqlServer { get; set; }
    }
}
