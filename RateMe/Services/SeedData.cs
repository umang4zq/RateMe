using System.Collections.Generic;
using RateMe.Models;

namespace RateMe.Services
{
    public static class SeedData
    {
        public static List<MediaItem> GetCuratedMedia()
        {
            return new List<MediaItem>
            {
                new MediaItem
                {
                    ImdbId = "tt1375666",
                    Title = "Inception",
                    Year = "2010",
                    Type = "movie",
                    Genre = "Adventure, Sci-Fi, Thriller",
                    Director = "Christopher Nolan",
                    Actors = "Leonardo DiCaprio, Joseph Gordon-Levitt, Elliot Page",
                    Plot = "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea into the mind of a C.E.O., but his tragic past may doom the project and his team to disaster.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BMjAxMzY3NjcxNF5BMl5BanBnXkFtZTcwNTI5OTM0Mw@@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 8.8m,
                    Metascore = 74,
                    Runtime = "148 min",
                    TotalUserRatings = 142,
                    AverageUserRating = 9.4m
                },
                new MediaItem
                {
                    ImdbId = "tt0468569",
                    Title = "The Dark Knight",
                    Year = "2008",
                    Type = "movie",
                    Genre = "Crime, Thriller, Action",
                    Director = "Christopher Nolan",
                    Actors = "Christian Bale, Heath Ledger, Aaron Eckhart",
                    Plot = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BMTMxNTMwODM0NF5BMl5BanBnXkFtZTcwODAyMTk2Mw@@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 9.1m,
                    Metascore = 84,
                    Runtime = "152 min",
                    TotalUserRatings = 210,
                    AverageUserRating = 9.7m
                },
                new MediaItem
                {
                    ImdbId = "tt0816692",
                    Title = "Interstellar",
                    Year = "2014",
                    Type = "movie",
                    Genre = "Adventure, Sci-Fi, Drama",
                    Director = "Christopher Nolan",
                    Actors = "Matthew McConaughey, Anne Hathaway, Jessica Chastain",
                    Plot = "When Earth becomes uninhabitable in the future, a farmer and ex-NASA pilot, Joseph Cooper, is tasked to pilot a spacecraft, along with a team of researchers, to find a new planet for humans.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BYzdjMDAxZGItMjI2My00ODA1LTlkNzItOWFjMDU5ZDJlYWY3XkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 8.7m,
                    Metascore = 74,
                    Runtime = "169 min",
                    TotalUserRatings = 188,
                    AverageUserRating = 9.3m
                },
                new MediaItem
                {
                    ImdbId = "tt0903747",
                    Title = "Breaking Bad",
                    Year = "2008–2013",
                    Type = "series",
                    Genre = "Crime, Drama, Thriller",
                    Director = "Vince Gilligan",
                    Actors = "Bryan Cranston, Aaron Paul, Anna Gunn",
                    Plot = "A chemistry teacher diagnosed with inoperable lung cancer turns to manufacturing and selling methamphetamine with a former student in order to secure his family's financial future.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BOWE4NTc3YmYtNmU2Mi00ZjhkLWE1MTItZmM1M2U1ODU3YjFlXkEyXkFqcGc@._V1_QL75_UY562_CR2,0,380,562_.jpg",
                    ImdbRating = 9.5m,
                    Metascore = null,
                    Runtime = "49 min",
                    TotalUserRatings = 320,
                    AverageUserRating = 9.8m
                },
                new MediaItem
                {
                    ImdbId = "tt4574334",
                    Title = "Stranger Things",
                    Year = "2016–2025",
                    Type = "series",
                    Genre = "Drama, Fantasy, Horror",
                    Director = "The Duffer Brothers",
                    Actors = "Millie Bobby Brown, Finn Wolfhard, Winona Ryder",
                    Plot = "When a young boy vanishes, a small town uncovers a mystery involving secret experiments, terrifying supernatural forces and one strange little girl.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BMjEzMDAxOTUyMV5BMl5BanBnXkFtZTgwNzAxMzYzOTE@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 8.6m,
                    Metascore = null,
                    Runtime = "51 min",
                    TotalUserRatings = 95,
                    AverageUserRating = 8.9m
                },
                new MediaItem
                {
                    ImdbId = "tt0111161",
                    Title = "The Shawshank Redemption",
                    Year = "1994",
                    Type = "movie",
                    Genre = "Drama",
                    Director = "Frank Darabont",
                    Actors = "Tim Robbins, Morgan Freeman, Bob Gunton",
                    Plot = "Over the course of several years, two convicts form a friendship, seeking consolation and, eventually, redemption through basic compassion.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BMDAyY2FhYjctNDc5OS00MDNlLThiMGUtY2UxYWVkNGY2ZjljXkEyXkFqcGc@._V1_QL75_UX380_CR0,4,380,562_.jpg",
                    ImdbRating = 9.3m,
                    Metascore = 82,
                    Runtime = "142 min",
                    TotalUserRatings = 240,
                    AverageUserRating = 9.6m
                },
                new MediaItem
                {
                    ImdbId = "tt0133093",
                    Title = "The Matrix",
                    Year = "1999",
                    Type = "movie",
                    Genre = "Action, Sci-Fi",
                    Director = "Lana Wachowski, Lilly Wachowski",
                    Actors = "Keanu Reeves, Laurence Fishburne, Carrie-Anne Moss",
                    Plot = "When a beautiful stranger leads computer hacker Neo to a forbidding underworld, he discovers the shocking truth--the life he knows is the elaborate deception of an evil cyber-intelligence.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BN2NmN2VhMTQtMDNiOS00NDlhLTliMjgtODE2ZTY0ODQyNDRhXkEyXkFqcGc@._V1_QL75_UX380_CR0,4,380,562_.jpg",
                    ImdbRating = 8.7m,
                    Metascore = 73,
                    Runtime = "136 min",
                    TotalUserRatings = 150,
                    AverageUserRating = 9.1m
                },
                new MediaItem
                {
                    ImdbId = "tt15398776",
                    Title = "Oppenheimer",
                    Year = "2023",
                    Type = "movie",
                    Genre = "Biography, Drama, History",
                    Director = "Christopher Nolan",
                    Actors = "Cillian Murphy, Emily Blunt, Matt Damon",
                    Plot = "The story of American scientist J. Robert Oppenheimer and his role in the development of the atomic bomb during World War II.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BN2JkMDc5MGQtZjg3YS00NmFiLWIyZmQtZTJmNTM5MjVmYTQ4XkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 8.2m,
                    Metascore = 88,
                    Runtime = "180 min",
                    TotalUserRatings = 115,
                    AverageUserRating = 9.2m
                }
            };
        }
    }
}
