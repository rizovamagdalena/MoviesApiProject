# 🍿 PopcornPass API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)

ASP.NET Core Web API for managing movie tickets and screenings for a cinema. This API serves as the backend for the PopcornPass web application, handling all cinema operations including movie management, user authentication, screening schedules, and ticket bookings.

## 🎬 Features

- **Movie Management**: Add, update, and retrieve movie information with detailed metadata
- **Future Movies**: Manage upcoming movie releases and announcements
- **User Management**: Handle user registration, authentication, and profiles
- **Screening Management**: Manage movie screenings across different cinema halls
- **Ticket Booking**: Process ticket reservations and seat selections
- **Hall & Seat Management**: Configure cinema halls and seat availability
- **Genre Classification**: Organize movies by multiple genres
- **Movie Ratings**: User-generated ratings and reviews
- **AI-Powered Features**: OpenAI integration for intelligent answers about information for movies and screenings

## 🏗️ Architecture

### Database Schema

The API uses PostgreSQL with the following tables:

| Table | Description |
|-------|-------------|
| `movie` | Stores movie information (title, description, duration, etc.) |
| `futuremovies` | Upcoming movies and release schedules |
| `genres` | Movie genre definitions |
| `moviegenres` | Many-to-many relationship between movies and genres |
| `screening` | Movie screening schedules |
| `hall` | Cinema hall configurations |
| `hall_seat` | Seat layouts for each hall |
| `seat_for_screening` | Seat availability for specific screenings |
| `ticket` | Booked tickets |
| `users` | User accounts and authentication |

### Technology Stack

- **Framework**: ASP.NET Core 8
- **ORM**: Dapper (lightweight ORM for high performance)
- **Database**: PostgreSQL
- **JSON Serialization**: Newtonsoft.Json
- **AI Integration**: OpenAI API (for intelligent features)
- **Authentication**: JWT (JSON Web Tokens)

### Project Structure

```
PopcornPassApiProject/
├── Controllers/        # API endpoints and request handling
├── Models/            # Data models and DTOs
├── Repositories/      # Data access layer (Dapper queries)
├── Service/          # Integrations with external APIs (OpenAI, SendinBlue email)
├── appsettings.json   # Configuration file
└── Program.cs         # Application entry point
```

**Architecture Pattern**: The project follows a layered architecture with separated concerns:
- **Controllers**: Handle HTTP requests and responses
- **Services**: Integrations with external APIs 
- **Repositories**: Manage database operations using Dapper
- **Models**: Define data structures

## 🚀 Getting Started

### Prerequisites

Ensure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) (version 12 or higher recommended)
- A code editor ([Visual Studio](https://visualstudio.microsoft.com/), [VS Code](https://code.visualstudio.com/), or [Rider](https://www.jetbrains.com/rider/))
- [OpenAI API Key](https://platform.openai.com/api-keys) (for AI-powered features)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/PopcornPassApiProject.git
   cd PopcornPassApiProject
   ```

2. **Configure the database connection**
   
   Update the connection string in `appsettings.json`:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
     "DBSettings": {
       "PostgresDB": "Host=localhost;Port=5432;Database=movieprojectdb;Username=postgres;Password=your_password"
     },
     "OpenAI": {
       "ApiKey": "your-openai-api-key"
     }
   }
   ```
   
   **Important**: Replace `your_password` and `your-openai-api-key` with your actual credentials.

3. **Set up the database**
   
   Create the PostgreSQL database:
   ```bash
   psql -U postgres
   CREATE DATABASE movieprojectdb;
   ```

   Run the database migration scripts (located in `/Database` or `/Scripts` folder if available):
   ```bash
   psql -U postgres -d movieprojectdb -f schema.sql
   ```

4. **Install dependencies**
   ```bash
   dotnet restore
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

   The API will start at `https://localhost:5001` (or the port specified in `launchSettings.json`)

### Environment Variables

For production deployments, consider using environment variables for sensitive configuration:

```bash
export DBSettings__PostgresDB="Host=your_host;Port=5432;Database=movieprojectdb;Username=user;Password=pass"
export OpenAI__ApiKey="your-openai-api-key"
export JWT__Secret="your-secret-key"
export JWT__Issuer="PopcornPassAPI"
export JWT__Audience="PopcornPassWeb"
```

**Note**: The API integrates with OpenAI for AI-powered features (movie recommendations, intelligent search, etc.).

## 📚 API Documentation

### Data Models

#### Movie Model
```json
{
  "id": 123,
  "name": "Movie Title",
  "duration": 120,
  "release_Date": "2024-01-15",
  "amount": 12.50,
  "poster_Path": "url/to/poster.jpg",
  "plot": "Movie description...",
  "actors": "Actor 1, Actor 2",
  "directors": "Director Name",
  "genres": "Action, Drama",
  "ratings": [],
  "rating": 8.5
}
```

#### Future Movie Model
```json
{
  "id": 456,
  "name": "Upcoming Movie",
  "genres": "Sci-Fi, Thriller",
  "poster_Path": "url/to/poster.jpg"
}
```

### Base URL
```
Development: https://localhost:5001/api
```

### Authentication

Most endpoints require authentication via JWT tokens. Include the token in the `Authorization` header:

```
Authorization: Bearer {your_jwt_token}
```

### Endpoints Overview

#### Movies
- `GET /api/movies` - Get all movies
- `GET /api/movies/{id}` - Get movie by ID
- `POST /api/movies` - Add a new movie *(Admin)*
- `PUT /api/movies/{id}` - Update movie *(Admin)*
- `DELETE /api/movies/{id}` - Delete movie *(Admin)*

#### Future Movies
- `GET /api/futuremovies` - Get upcoming movies
- `POST /api/futuremovies` - Add a future movie *(Admin)*
- `PUT /api/futuremovies/{id}` - Update future movie *(Admin)*
- `DELETE /api/futuremovies/{id}` - Delete future movie *(Admin)*

#### Screenings
- `GET /api/screenings` - Get all screenings
- `GET /api/screenings/{id}` - Get screening by ID
- `GET /api/screenings/movie/{movieId}` - Get screenings for a specific movie
- `POST /api/screenings` - Create a new screening *(Admin)*
- `PUT /api/screenings/{id}` - Update screening *(Admin)*
- `DELETE /api/screenings/{id}` - Delete screening *(Admin)*

#### Tickets
- `GET /api/tickets/user/{userId}` - Get user's tickets
- `POST /api/tickets` - Book a ticket
- `DELETE /api/tickets/{id}` - Cancel a ticket

#### Users
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and receive JWT token
- `GET /api/users/profile` - Get current user profile
- `PUT /api/users/profile` - Update user profile

#### Halls & Seats
- `GET /api/halls` - Get all cinema halls
- `GET /api/halls/{id}/seats` - Get seat layout for a hall
- `GET /api/screenings/{screeningId}/available-seats` - Get available seats for a screening



## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.


**Note**: This API is designed to work in conjunction with the [PopcornPass Web Application](https://github.com/yourusername/PopcornPassWebProject). Make sure both projects are properly configured and connected.
