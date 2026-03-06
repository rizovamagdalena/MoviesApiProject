using MoviesAPI.Models.System;

namespace MoviesAPI.Models
{
    public abstract class MovieBase
    {
        public string Name { get; set; }
        public int Duration { get; set; }
        public DateTime Release_Date { get; set; }
        public decimal Amount { get; set; }
        public string Poster_Path { get; set; }
        public string Plot { get; set; }
        public string Actors { get; set; }
        public string Directors { get; set; }
    }
    public class Movie : MovieBase
    {
        public long Id { get; set; }
        public List<string> Genres { get; set; }
        public List<MovieRating> Ratings { get; set; } = new();
        public decimal Rating { get; set; }
    }

    public class CreateAndUpdateMovie : MovieBase
    {
        public List<string> Genres { get; set; }
    }



    public class FutureMovie
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Genres { get; set; }
        public string Poster_Path { get; set; }
    }


    public class CreateFutureMovie
    {
        public string Name { get; set; }
        public string Genres { get; set; }
        public string Poster_Path { get; set; }
    }

}
