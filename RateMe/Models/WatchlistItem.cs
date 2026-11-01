using System;

namespace RateMe.Models
{
    public class WatchlistItem
    {
        public int WatchlistId { get; set; }
        public int UserId { get; set; }
        public int MediaId { get; set; }
        public string Status { get; set; } = "PlanToWatch"; // "PlanToWatch", "Watching", "Completed", "Dropped"
        public DateTime AddedAt { get; set; } = DateTime.Now;

        // Navigation property
        public MediaItem Media { get; set; }
    }
}

