using Dapper;
using Microsoft.Extensions.Options;
using MoviesAPI.Models;
using MoviesAPI.Models.System;
using MoviesAPI.Repositories.Interface;
using Npgsql;
using System.Net.Sockets;

namespace MoviesAPI.Repositories.Implementation
{
    public class ScreeningRepository : IScreeningRepository
    {
        private readonly DBSettings _dbSettings;

        public ScreeningRepository(IOptions<DBSettings> dbSettings)
        {
            _dbSettings = dbSettings.Value;
        }

        public async Task<IEnumerable<ScreeningResponse>> GetScreeningsAsync()
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            string sql = @"SELECT 
                        s.id,
                        s.movie_id, 
                        m.name AS movie_name,
                        s.screening_date_time,
                        s.total_tickets, 
                        s.available_tickets,
                        s.hall_id
                        FROM screening s
                        INNER JOIN movie m ON s.movie_id = m.id;";

            var result = await conn.QueryAsync<ScreeningResponse>(sql);
          
            return result;
        }

        public async Task<ScreeningResponse> GetScreeningAsync(long id)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            string sql = @"
                SELECT 
                    s.id,
                    s.movie_id,
                    s.screening_date_time,
                    s.total_tickets,
                    s.available_tickets,
                    s.hall_id,
                    m.id AS MovieId,
                    m.name AS Name,
                    m.amount AS Amount
                FROM screening s
                INNER JOIN movie m ON s.movie_id = m.id
                WHERE s.id = @id;
            ";
            var screening = await conn.QueryAsync<ScreeningResponse, MovieSummary, ScreeningResponse>(
                sql,
                (s, m) =>
                {
                    s.Movie = m;
                    return s;
                },
                new { id },
                splitOn: "MovieId"
            );

            return screening.FirstOrDefault();
        }

        public async Task<Screening> GetScreeningForUpdateAsync(long id)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            string sql = "SELECT id, movie_id, screening_date_time, total_tickets, available_tickets FROM screening WHERE id = @id";

            var updateScreening = await conn.QueryFirstOrDefaultAsync<Screening>(sql, new { id });
            return updateScreening;

        }

        public async Task<int> CreateScreeningAsync(CreateScreening screening)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            var numRowAndSeatSql = "SELECT rows,seats_per_row FROM hall WHERE id = @HallId;";
            var numRowAndSeat = await conn.QueryFirstOrDefaultAsync<(int rows, int seats_per_row)>(numRowAndSeatSql, new { HallId = screening.Hall_Id });

            int totalSeats = numRowAndSeat.rows * numRowAndSeat.seats_per_row;

            var ticketPrice = await conn.ExecuteScalarAsync<decimal>(
                "SELECT amount FROM movie WHERE id = @Id", new { Id = screening.Movie_Id });

            string sql = @"
                INSERT INTO screening(movie_id, screening_date_time, total_tickets, available_tickets, hall_id, ticket_price)
                VALUES (@Movie_Id, @Screening_Date_Time, @TotalTickets, @AvailableTickets, @Hall_Id, @Ticket_Price)
                RETURNING id;
             ";

            var id = await conn.ExecuteScalarAsync<int>(sql, new
            {
                screening.Movie_Id,
                screening.Screening_Date_Time,
                TotalTickets = totalSeats,
                AvailableTickets = totalSeats,
                screening.Hall_Id,
                Ticket_Price = ticketPrice
            });

            return id;


        }

        public async Task<int> UpdateScreeningAsync(long id, UpdateScreening screening)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = @"UPDATE screening
                   SET movie_id = @Movie_Id,
                       screening_date_time = @Screening_Date_Time,
                       total_tickets = @Total_Tickets,
                       available_tickets = @Available_Tickets
                       WHERE id = @Id";

            return await conn.ExecuteAsync(sql, new
            {
                Id = id,
                screening.Movie_Id,
                screening.Screening_Date_Time,
                screening.Total_Tickets,
                screening.Available_Tickets
            });
        }

        public async Task<int> DeleteScreeningAsync(long id)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = "delete from screening where id = @id";

            return await conn.ExecuteAsync(sql, new { id });

        }

        public async Task<List<SeatForScreeningDto>> GetReservedSeatsAsync(long screeningId)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = @"
                SELECT sfs.id AS Id, sfs.screening_id AS ScreeningId, 
                       hs.id AS HallSeatId, hs.row_number, hs.seat_number, sfs.user_id AS UserId
                FROM seat_for_screening sfs
                INNER JOIN hall_seat hs ON hs.id = sfs.hall_seat_id
                WHERE sfs.screening_id = @ScreeningId
                ORDER BY hs.row_number, hs.seat_number;
              ";

            var seats = await conn.QueryAsync<SeatForScreeningDto>(sql, new { ScreeningId = screeningId });
            return seats.ToList();
        }


        public async Task BookSeatsAsync(long screeningId, string username, List<int> hallSeatIds)
        {

            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string userIdSql = @"SELECT id FROM users WHERE username = @Username";
                var userId = await conn.ExecuteScalarAsync<long?>(userIdSql, new { Username = username }, transaction);

                if (userId == null)
                    throw new Exception($"User '{username}' not found.");

                long actualUserId = userId.Value;

                string conflictSql = @"
                    SELECT hall_seat_id 
                    FROM seat_for_screening 
                    WHERE screening_id = @ScreeningId 
                      AND hall_seat_id = ANY(@SeatIds)";

                var takenSeats = (await conn.QueryAsync<int>(
                    conflictSql,
                    new { ScreeningId = screeningId, SeatIds = hallSeatIds },
                    transaction)).ToList();

                if (takenSeats.Any())
                    throw new Exception($"Seats already booked: {string.Join(", ", takenSeats)}");

                ScreeningResponse screening = await GetScreeningAsync(screeningId);
                decimal ticketAmount = screening.Movie.Amount;

                foreach (var seatId in hallSeatIds)
                {
                    string ticketSql = @"
                    INSERT INTO ticket(movie_id, user_id, watch_movie, price, hall_seat_id)
                    VALUES(@MovieId, @UserId, @WatchMovie, @Price, @HallSeatId)
                    RETURNING id;";

                    int ticketId = await conn.ExecuteScalarAsync<int>(
                        ticketSql,
                        new
                        {
                            MovieId = screening.Movie_Id,
                            UserId = actualUserId,
                            WatchMovie = screening.Screening_Date_Time,
                            Price = ticketAmount,
                            HallSeatId = seatId
                        },
                        transaction);

                    string insertSeatSql = @"
                INSERT INTO seat_for_screening(screening_id, hall_seat_id, user_id, ticket_id)
                VALUES(@ScreeningId, @HallSeatId, @UserId, @TicketId);";

                    await conn.ExecuteAsync(insertSeatSql,
                        new { ScreeningId = screeningId, HallSeatId = seatId, UserId = actualUserId, TicketId = ticketId },
                        transaction);
                }

                string updateSql = @"
            UPDATE screening 
            SET available_tickets = available_tickets - @Count 
            WHERE id = @ScreeningId";

                await conn.ExecuteAsync(updateSql,
                    new { Count = hallSeatIds.Count, ScreeningId = screeningId },
                    transaction);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Screening>> GetScreeningsByHallAndDateAsync(int hall_id,DateOnly date)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = @"
                SELECT *
                FROM screening s
                WHERE s.hall_id = @hall_id
                  AND DATE(s.screening_date_time) = @screening_date;
            ";

            var screenings = await conn.QueryAsync<Screening>(
                sql,
                new { hall_id, screening_date = date.ToDateTime(TimeOnly.MinValue) }
            );

            return screenings.ToList();
        }





      
    }
}
