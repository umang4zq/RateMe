using System;

namespace RateMe.Models
{
    public class MediaItem
    {
        public int MediaId { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Type { get; set; } = "movie"; // "movie", "series", "episode"
        public string Genre { get; set; }
        public string Director { get; set; }
        public string Actors { get; set; }
        public string Plot { get; set; }
        public string PosterUrl { get; set; }
        public decimal? ImdbRating { get; set; }
        public int? Metascore { get; set; }
        public string Runtime { get; set; }
        public int TotalUserRatings { get; set; }
        public decimal AverageUserRating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public string DisplayScore
        {
            get
            {
                if (AverageUserRating > 0)
                    return AverageUserRating.ToString("0.0");
                if (ImdbRating.HasValue)
                    return ImdbRating.Value.ToString("0.0");
                return "NR";
            }
        }
    }
}

