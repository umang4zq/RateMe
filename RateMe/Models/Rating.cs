using System;

namespace RateMe.Models
{
    public class Rating
    {
        public int RatingId { get; set; }
        public int UserId { get; set; }
        public int MediaId { get; set; }
        public decimal Score { get; set; } // 1.0 to 10.0
        public string ReviewText { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Joined / Navigation properties for UI display
        public string Username { get; set; }
        public string UserDisplayName { get; set; }
        public string MediaTitle { get; set; }
        public string MediaImdbId { get; set; }
        public string MediaPosterUrl { get; set; }
    }
}

