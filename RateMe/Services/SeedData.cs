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
                // ==========================================
                // 15 MASTERPIECE MOVIES (3 full 5-card rows)
                // ==========================================
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
                    ImdbRating = 8.9m,
                    Metascore = 88,
                    Runtime = "180 min",
                    TotalUserRatings = 115,
                    AverageUserRating = 9.2m
                },
                new MediaItem
                {
                    ImdbId = "tt0110912",
                    Title = "Pulp Fiction",
                    Year = "1994",
                    Type = "movie",
                    Genre = "Crime, Drama",
                    Director = "Quentin Tarantino",
                    Actors = "John Travolta, Uma Thurman, Samuel L. Jackson",
                    Plot = "The lives of two mob hitmen, a boxer, a gangster and his wife, and a pair of diner bandits intertwine in four tales of violence and redemption.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BYTViYTE3ZGQtNDBlMC00ZTAyLTkyODMtZGRiZDg0MjA2YThkXkEyXkFqcGc@._V1_QL75_UY562_CR3,0,380,562_.jpg",
                    ImdbRating = 8.8m,
                    Metascore = 95,
                    Runtime = "154 min",
                    TotalUserRatings = 195,
                    AverageUserRating = 9.5m
                },
                new MediaItem
                {
                    ImdbId = "tt0137523",
                    Title = "Fight Club",
                    Year = "1999",
                    Type = "movie",
                    Genre = "Crime, Drama, Thriller",
                    Director = "David Fincher",
                    Actors = "Brad Pitt, Edward Norton, Meat Loaf",
                    Plot = "An insomniac office worker and a devil-may-care soap maker form an underground fight club that evolves into much more.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BOTgyOGQ1NDItNGU3Ny00MjU3LTg2YWEtNmEyYjBiMjI1Y2M5XkEyXkFqcGc@._V1_QL75_UX380_CR0,4,380,562_.jpg",
                    ImdbRating = 8.8m,
                    Metascore = 67,
                    Runtime = "139 min",
                    TotalUserRatings = 175,
                    AverageUserRating = 9.3m
                },
                new MediaItem
                {
                    ImdbId = "tt0109830",
                    Title = "Forrest Gump",
                    Year = "1994",
                    Type = "movie",
                    Genre = "Drama, Romance",
                    Director = "Robert Zemeckis",
                    Actors = "Tom Hanks, Robin Wright, Gary Sinise",
                    Plot = "The history of the United States from the 1950s to the '70s unfolds from the perspective of an Alabama man with an IQ of 75, who yearns to be reunited with his childhood sweetheart.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BNDYwNzVjMTItZmU5YS00YjQ5LTljYjgtMjY2NDVmYWMyNWFmXkEyXkFqcGc@._V1_QL75_UY562_CR4,0,380,562_.jpg",
                    ImdbRating = 8.8m,
                    Metascore = 82,
                    Runtime = "142 min",
                    TotalUserRatings = 210,
                    AverageUserRating = 9.4m
                },
                new MediaItem
                {
                    ImdbId = "tt0068646",
                    Title = "The Godfather",
                    Year = "1972",
                    Type = "movie",
                    Genre = "Crime, Drama",
                    Director = "Francis Ford Coppola",
                    Actors = "Marlon Brando, Al Pacino, James Caan",
                    Plot = "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant son.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BNGEwYjgwOGQtYjg5ZS00Njc1LTk2ZGEtM2QwZWQ2NjdhZTE5XkEyXkFqcGc@._V1_QL75_UY562_CR8,0,380,562_.jpg",
                    ImdbRating = 9.2m,
                    Metascore = 100,
                    Runtime = "175 min",
                    TotalUserRatings = 280,
                    AverageUserRating = 9.9m
                },
                new MediaItem
                {
                    ImdbId = "tt0120737",
                    Title = "The Lord of the Rings: The Fellowship of the Ring",
                    Year = "2001",
                    Type = "movie",
                    Genre = "Action, Adventure, Drama",
                    Director = "Peter Jackson",
                    Actors = "Elijah Wood, Ian McKellen, Orlando Bloom",
                    Plot = "A meek Hobbit from the Shire and eight companions set out on a journey to destroy the powerful One Ring and save Middle-earth from the Dark Lord Sauron.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BNzIxMDQ2YTctNDY4MC00ZTRhLTk4ODQtMTVlOWY4NTdiYmMwXkEyXkFqcGc@._V1_QL75_UX380_CR0,1,380,562_.jpg",
                    ImdbRating = 8.9m,
                    Metascore = 92,
                    Runtime = "178 min",
                    TotalUserRatings = 230,
                    AverageUserRating = 9.6m
                },
                new MediaItem
                {
                    ImdbId = "tt15239678",
                    Title = "Dune: Part Two",
                    Year = "2024",
                    Type = "movie",
                    Genre = "Action, Adventure, Drama",
                    Director = "Denis Villeneuve",
                    Actors = "Timothée Chalamet, Zendaya, Rebecca Ferguson",
                    Plot = "Paul Atreides unites with Chani and the Fremen while seeking revenge against the conspirators who destroyed his family.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BNTc0YmQxMjEtODI5MC00NjFiLTlkMWUtOGQ5NjFmYWUyZGJhXkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 8.5m,
                    Metascore = 79,
                    Runtime = "166 min",
                    TotalUserRatings = 160,
                    AverageUserRating = 9.3m
                },
                new MediaItem
                {
                    ImdbId = "tt0172495",
                    Title = "Gladiator",
                    Year = "2000",
                    Type = "movie",
                    Genre = "Action, Adventure, Drama",
                    Director = "Ridley Scott",
                    Actors = "Russell Crowe, Joaquin Phoenix, Connie Nielsen",
                    Plot = "A former Roman General sets out to exact vengeance against the corrupt emperor who murdered his family and sent him into slavery.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BYWQ4YmNjYjEtOWE1Zi00Y2U4LWI4NTAtMTU0MjkxNWQ1ZmJiXkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 8.5m,
                    Metascore = 67,
                    Runtime = "155 min",
                    TotalUserRatings = 170,
                    AverageUserRating = 9.2m
                },
                new MediaItem
                {
                    ImdbId = "tt0245429",
                    Title = "Spirited Away",
                    Year = "2001",
                    Type = "movie",
                    Genre = "Animation, Adventure, Family",
                    Director = "Hayao Miyazaki",
                    Actors = "Daveigh Chase, Suzanne Pleshette, Miyu Irino",
                    Plot = "During her family's move to the suburbs, a sullen 10-year-old girl wanders into a world ruled by gods, witches, and spirits, a world where humans are changed into beasts.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BNTEyNmEwOWUtYzkyOC00ZTQ4LTllZmUtMjk0Y2YwOGUzYjRiXkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 8.6m,
                    Metascore = 96,
                    Runtime = "125 min",
                    TotalUserRatings = 145,
                    AverageUserRating = 9.4m
                },
                new MediaItem
                {
                    ImdbId = "tt9362722",
                    Title = "Spider-Man: Across the Spider-Verse",
                    Year = "2023",
                    Type = "movie",
                    Genre = "Animation, Action, Adventure",
                    Director = "Joaquim Dos Santos, Kemp Powers, Justin K. Thompson",
                    Actors = "Shameik Moore, Hailee Steinfeld, Brian Tyree Henry",
                    Plot = "Miles Morales catapults across the Multiverse, where he encounters a team of Spider-People charged with protecting its very existence.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BNThiZjA3MjItZGY5Ni00ZmJhLWEwN2EtOTBlYTA4Y2E0M2ZmXkEyXkFqcGc@._V1_QL75_UX380_CR0,1,380,562_.jpg",
                    ImdbRating = 8.6m,
                    Metascore = 86,
                    Runtime = "140 min",
                    TotalUserRatings = 155,
                    AverageUserRating = 9.3m
                },

                // ==========================================
                // 10 ACCLAIMED TV SERIES (2 full 5-card rows)
                // ==========================================
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
                    Metascore = 99,
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
                    ImdbRating = 8.7m,
                    Metascore = 76,
                    Runtime = "51 min",
                    TotalUserRatings = 95,
                    AverageUserRating = 8.9m
                },
                new MediaItem
                {
                    ImdbId = "tt0944947",
                    Title = "Game of Thrones",
                    Year = "2011–2019",
                    Type = "series",
                    Genre = "Drama, Fantasy, Action",
                    Director = "David Benioff, D.B. Weiss",
                    Actors = "Emilia Clarke, Peter Dinklage, Kit Harington",
                    Plot = "Nine noble families fight for control over the lands of Westeros, while an ancient enemy returns after being dormant for millennia.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BNGYxOGJkMjItZjVkZC00OGEzLWExNjktOTZmNGZhZmRlMTk2XkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 9.2m,
                    Metascore = 86,
                    Runtime = "57 min",
                    TotalUserRatings = 290,
                    AverageUserRating = 9.5m
                },
                new MediaItem
                {
                    ImdbId = "tt8770514",
                    Title = "Chernobyl",
                    Year = "2019",
                    Type = "series",
                    Genre = "Drama, History, Thriller",
                    Director = "Craig Mazin",
                    Actors = "Jessie Buckley, Jared Harris, Stellan Skarsgård",
                    Plot = "In April 1986, an explosion at the Chernobyl nuclear power plant in the Union of Soviet Socialist Republics becomes one of the world's worst man-made catastrophes.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BNTEzNjBiMTAtM2NiYi00N2Q4LTljMjgtZGMxNTZmY2IxNzE4XkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 9.3m,
                    Metascore = 82,
                    Runtime = "330 min",
                    TotalUserRatings = 210,
                    AverageUserRating = 9.7m
                },
                new MediaItem
                {
                    ImdbId = "tt3032476",
                    Title = "Better Call Saul",
                    Year = "2015–2022",
                    Type = "series",
                    Genre = "Crime, Drama",
                    Director = "Vince Gilligan, Peter Gould",
                    Actors = "Bob Odenkirk, Rhea Seehorn, Jonathan Banks",
                    Plot = "The trials and tribulations of criminal lawyer Jimmy McGill in the years leading up to his fateful run-in with Walter White and Jesse Pinkman.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BNDJhYTk2ZTItZTI3Yi00OWQ1LWFkNmUtMGU3NTRjOGE4MGQ0XkEyXkFqcGc@._V1_QL75_UX380_CR0,4,380,562_.jpg",
                    ImdbRating = 9.0m,
                    Metascore = 86,
                    Runtime = "46 min",
                    TotalUserRatings = 180,
                    AverageUserRating = 9.5m
                },
                new MediaItem
                {
                    ImdbId = "tt0141842",
                    Title = "The Sopranos",
                    Year = "1999–2007",
                    Type = "series",
                    Genre = "Crime, Drama",
                    Director = "David Chase",
                    Actors = "James Gandolfini, Lorraine Bracco, Edie Falco",
                    Plot = "New Jersey mob boss Tony Soprano deals with personal and professional issues in his home and business life that's affecting his mental state.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BNmQ4YmE0M2UtNDk5Yi00MzFhLWEwZmMtNzg4YmE5ZGU5Y2E3XkEyXkFqcGc@._V1_QL75_UX380_CR0,4,380,562_.jpg",
                    ImdbRating = 9.2m,
                    Metascore = 97,
                    Runtime = "55 min",
                    TotalUserRatings = 240,
                    AverageUserRating = 9.7m
                },
                new MediaItem
                {
                    ImdbId = "tt11280740",
                    Title = "Severance",
                    Year = "2022–",
                    Type = "series",
                    Genre = "Drama, Mystery, Sci-Fi",
                    Director = "Dan Erickson",
                    Actors = "Adam Scott, Zach Cherry, Britt Lower",
                    Plot = "Mark leads a team of office workers whose memories have been surgically divided between their work and personal lives.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BMjkwZjcwMGQtNDAzOC00NWVkLTk1MTktNDlhNTBkZGYwN2RkXkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 8.7m,
                    Metascore = 83,
                    Runtime = "50 min",
                    TotalUserRatings = 135,
                    AverageUserRating = 9.2m
                },
                new MediaItem
                {
                    ImdbId = "tt7660850",
                    Title = "Succession",
                    Year = "2018–2023",
                    Type = "series",
                    Genre = "Drama",
                    Director = "Jesse Armstrong",
                    Actors = "Nicholas Braun, Brian Cox, Kieran Culkin",
                    Plot = "The Roy family is known for controlling the biggest media and entertainment company in the world. However, their world changes when their father steps down from the company.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BYTY4YTVkY2QtMjRmOS00YzliLWIxOWQtMTdkOTVkN2UzODNmXkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 8.9m,
                    Metascore = 89,
                    Runtime = "60 min",
                    TotalUserRatings = 165,
                    AverageUserRating = 9.4m
                },
                new MediaItem
                {
                    ImdbId = "tt3581920",
                    Title = "The Last of Us",
                    Year = "2023–",
                    Type = "series",
                    Genre = "Action, Adventure, Drama",
                    Director = "Neil Druckmann, Craig Mazin",
                    Actors = "Pedro Pascal, Bella Ramsey, Gabriel Luna",
                    Plot = "After a global pandemic destroys civilization, a hardened survivor takes charge of a 14-year-old girl who may be humanity's last hope.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BYWI3ODJlMzktY2U5NC00ZjdlLWE1MGItNWQxZDk3NWNjN2RhXkEyXkFqcGc@._V1_QL75_UX380_CR0,0,380,562_.jpg",
                    ImdbRating = 8.7m,
                    Metascore = 84,
                    Runtime = "55 min",
                    TotalUserRatings = 150,
                    AverageUserRating = 9.3m
                },
                new MediaItem
                {
                    ImdbId = "tt2442560",
                    Title = "Peaky Blinders",
                    Year = "2013–2022",
                    Type = "series",
                    Genre = "Crime, Drama",
                    Director = "Steven Knight",
                    Actors = "Cillian Murphy, Paul Anderson, Sophie Rundle",
                    Plot = "A gangster family epic set in 1919 Birmingham, England; centered on a gang who sew razor blades in the peaks of their caps, and their fierce boss Tommy Shelby.",
                    PosterUrl = "https://m.media-amazon.com/images/M/MV5BOGM0NGY3ZmItOGE2ZC00OWIxLTk0N2EtZWY4Yzg3ZDlhNGI3XkEyXkFqcGc@._V1_QL75_UX380_CR0,4,380,562_.jpg",
                    ImdbRating = 8.8m,
                    Metascore = 77,
                    Runtime = "60 min",
                    TotalUserRatings = 175,
                    AverageUserRating = 9.4m
                }
            };
        }
    }
}
