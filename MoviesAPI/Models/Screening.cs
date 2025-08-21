namespace MoviesAPI.Models
{
    public class Screening
    {
        public int Id { get; set; }
        public int Movie_Id { get; set; }
        public DateTime Screening_Date_Time { get; set; }
        public int Total_Tickets { get; set; }
        public int Available_Tickets { get; set; }
        public int Hall_Id { get; set; }
    }

    public class CreateScreening
    {
        public int Movie_Id { get; set; }
        public DateTime Screening_Date_Time { get; set; }
        public int Hall_Id { get; set; }

    }

    public class UpdateScreening
    {
        public int Movie_Id { get; set; }
        public DateTime Screening_Date_Time { get; set; }
        public int Total_Tickets { get; set; }
        public int Available_Tickets { get; set; }
    }

    public class ScreeningResponse
    {
        public long Id { get; set; }
        public int Movie_Id { get; set; }
        public MovieSummary Movie { get; set; }
        public DateTime Screening_Date_Time { get; set; }
        public int Total_Tickets { get; set; }
        public int Available_Tickets { get; set; }
        public int Hall_Id { get; set; }

    }

    public class MovieSummary
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Poster_Path { get; set; }
        public decimal Amount { get; set; }
    }


    public class SeatForScreeningDto
    {
        public int Id { get; set; }             
        public int ScreeningId { get; set; }    
        public int HallSeatId { get; set; }     
        public int RowNumber { get; set; }      
        public int SeatNumber { get; set; }     
        public long? UserId { get; set; }       
    }

    public class BookSeatsRequest
    {
        public string username { get; set; }
        public List<int> SelectedSeatsId { get; set; }
    }

    
    
}
