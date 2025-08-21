# MoviesAPI
ASP.NET Core Web API for the **PopcornPass** web application, which is used to manage movie ticket sales and screenings at a cinema.  

This API provides endpoints for:  
- Managing movies (add, get, update)  
- Managing users  
- Handling screenings and tickets  
- Booking tickets  

---

## Database Models

The API uses PostgreSQL and includes the following tables:

- futuremovies  
- genres  
- hall  
- hall_seat  
- movie  
- moviegenres  
- screening  
- seat_for_screening  
- ticket  
- users

---

## Technologies Used
- **ASP.NET Core 8**  
- **Dapper**  
- **PostgreSQL**  
- **Newtonsoft.Json**  

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)  
- PostgreSQL database
