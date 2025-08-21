namespace MoviesAPI.Models
{
    public class Ticket
    {
        public long Id { get; set; }
        public long Movie_Id { get; set; }
        public long User_Id { get; set; }
        public DateTime Watch_Movie {  get; set; }
        public decimal Price { get; set; }
        public int hall_seat_id { get; set; }

    }

    public class TicketResponse
    {
        public long Id { get; set; }
        public string MovieName { get; set; }
        public string UserName { get; set; }
        public string PosterPath { get; set; }      

        public DateTime Watch_Movie { get; set; }
        public decimal Price { get; set; }
        public string HallName { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }





    public class CreateTicket
    {
        public long Movie_Id { get; set; }
        public long User_Id { get; set; }
        public DateTime Watch_Movie { get; set; }
        public decimal Price { get; set; }
        public int hall_seat_id { get; set; }

    }

    //public class UpdateTicket
    //{
    //    public DateTime Watch_Movie { get; set; }
    //    public decimal Price { get; set; }
    //}

    
}
