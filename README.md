# 🍿 PopcornPass application API

ASP.NET Core Web API for managing movie tickets and screenings for a cinema. This API serves as the backend for the PopcornPass web application, handling all cinema operations including movie management, user authentication, screening schedules, and ticket bookings.

## 🎬 Features
- **Movie Management**: Add, update, and retrieve movie information
- **Future Movies**: Manage upcoming movie releases and announcements
- **User Management**: Handle user registration and profiles
- **Screening Management**: Manage movie screenings across different cinema halls
- **Ticket Booking**: Process ticket reservations and seat selections
- **Hall & Seat Management**: Configure cinema halls and seat availability
- **Genre Classification**: Organize movies by multiple genres
- **Movie Ratings**: User-generated ratings and reviews
- **AI-Powered Features**: OpenAI integration for intelligent answers about information for movies and screenings
- **Email Confirmation** — Registration via email

## 🏗️ Architecture
The project follows a **Clean Layered Architecture** with clearly separated layers, each communicating only with the layer directly below it through interfaces. The project follows OOP and SOLID Principles.
```
┌─────────────────────────────────┐
│         Controllers             │  HTTP requests & responses only
├─────────────────────────────────┤
│           Services              │  Business logic & orchestration
├─────────────────────────────────┤
│          Repositories           │  SQL queries & data access
│                │                │ 
│         PostgreSQL              │  Database
└─────────────────────────────────┘
```

### Project Structure
```
MoviesApiProject/
├── Controllers/       # API endpoints and request handling
├── Models/            # Data models and DTOs
├── Repositories/      # Data access layer (Dapper queries)
├── Service/           # Business logic layer
├── appsettings.json   # Configuration file
├── Program.cs         # Application entry point
MoviesAPI.Tests        # Unit test project
```

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
- **Unit Testing**: xUnit, Moq


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

   The API will start at `https://localhost:7200` (or the port specified in `launchSettings.json`)

### Environment Variables

For production deployments, consider using environment variables for sensitive configuration:

```bash
export DBSettings__PostgresDB="Host=your_host;Port=5432;Database=movieprojectdb;Username=user;Password=pass"
export OpenAI__ApiKey="your-openai-api-key"
```

**Note**: The API integrates with OpenAI for AI-powered features (movie recommendations, intelligent search, etc.).



## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.


**Note**: This API is designed to work in conjunction with the [PopcornPass Web Application](https://github.com/yourusername/PopcornPassWebProject). Make sure both projects are properly configured and connected.
