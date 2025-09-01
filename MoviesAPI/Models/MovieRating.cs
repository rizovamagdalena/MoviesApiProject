namespace MoviesAPI.Models
{
    public class MovieRating
    {
        public int Id { get; set; }  
        public long UserId { get; set; }
        public long MovieId { get; set; }    
        public int Rating { get; set; }   
        public string Comment { get; set; }
        public string UserName { get; set; }

    }

    public class CreateRating {
        public long UserId { get; set; }
        public long MovieId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

}
