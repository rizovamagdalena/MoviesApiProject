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
        private readonly IDbConnectionFactory _connectionFactory;

        public ScreeningRepository(IDbConnectionFactory connection)
        {
            _connectionFactory = connection;
        }


        public async Task<IEnumerable<ScreeningResponse>> GetAllAsync()
        {
            using var conn = _connectionFactory.CreateConnection();
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

        public async Task<ScreeningResponse> GetByIdAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();
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

        public async Task<Screening> GetForUpdateAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = "SELECT id, movie_id, screening_date_time, total_tickets, available_tickets FROM screening WHERE id = @id";

            var updateScreening = await conn.QueryFirstOrDefaultAsync<Screening>(sql, new { id });
            return updateScreening;

        }

        public async Task<int> CreateAsync(CreateScreening screening)
        {
            using var conn = _connectionFactory.CreateConnection();

            var hallSql = "SELECT rows, seats_per_row FROM hall WHERE id = @HallId;";
            var hall = await conn.QueryFirstOrDefaultAsync<(int rows, int seats_per_row)>(
                hallSql, new { HallId = screening.Hall_Id });
            if (hall == default)
                throw new Exception($"Hall with ID {screening.Hall_Id} not found.");

            int totalSeats = hall.rows * hall.seats_per_row;

            var ticketPrice = await conn.ExecuteScalarAsync<decimal>(
                "SELECT amount FROM movie WHERE id = @Id;", new { Id = screening.Movie_Id });

            string sql = @"
                INSERT INTO screening(movie_id, screening_date_time, total_tickets, available_tickets, hall_id, ticket_price)
                VALUES (@Movie_Id, @Screening_Date_Time, @TotalTickets, @AvailableTickets, @Hall_Id, @Ticket_Price)
                RETURNING id;";

            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                screening.Movie_Id,
                screening.Screening_Date_Time,
                TotalTickets = totalSeats,
                AvailableTickets = totalSeats,
                screening.Hall_Id,
                Ticket_Price = ticketPrice
            });
        }

        public async Task<int> UpdateAsync(long id, UpdateScreening screening)
        {
            using var conn = _connectionFactory.CreateConnection();

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

        public async Task<int> DeleteAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();

            string sql = "DELETE FROM screening WHERE id = @id";

            return await conn.ExecuteAsync(sql, new { id });

        }

        public async Task<List<SeatForScreeningDto>> GetReservedSeatsAsync(long screeningId)
        {
            using var conn = _connectionFactory.CreateConnection();

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

        public async Task<List<int>> GetTakenSeatIdsAsync(long screeningId, List<int> hallSeatIds,
            NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            string sql = @"
                SELECT hall_seat_id FROM seat_for_screening
                WHERE screening_id = @ScreeningId AND hall_seat_id = ANY(@SeatIds);";

            return (await conn.QueryAsync<int>(sql,
                new { ScreeningId = screeningId, SeatIds = hallSeatIds },
                transaction)).ToList();
        }

        public async Task InsertSeatForScreeningAsync(long screeningId, int hallSeatId,
            long userId, int ticketId, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            string sql = @"
                INSERT INTO seat_for_screening(screening_id, hall_seat_id, user_id, ticket_id)
                VALUES(@ScreeningId, @HallSeatId, @UserId, @TicketId);";

            await conn.ExecuteAsync(sql,
                new { ScreeningId = screeningId, HallSeatId = hallSeatId, UserId = userId, TicketId = ticketId },
                transaction);
        }

        public async Task DecrementAvailableTicketsAsync(long screeningId, int count,
            NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            string sql = @"
                UPDATE screening 
                SET available_tickets = available_tickets - @Count 
                WHERE id = @ScreeningId;";

            await conn.ExecuteAsync(sql,
                new { Count = count, ScreeningId = screeningId },
                transaction);
        }

        public async Task<List<Screening>> GetByHallAndDateAsync(int hallId, DateOnly date)
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = @"
                SELECT * FROM screening
                WHERE hall_id = @HallId AND DATE(screening_date_time) = @Date;";

            return (await conn.QueryAsync<Screening>(sql,
                new { HallId = hallId, Date = date.ToDateTime(TimeOnly.MinValue) })).ToList();
        }

        public async Task<List<Screening>> GetByDateAsync(DateOnly date)
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = @"
                SELECT * FROM screening
                WHERE DATE(screening_date_time) = @Date;";

            return (await conn.QueryAsync<Screening>(sql,
                new { Date = date.ToDateTime(TimeOnly.MinValue) })).ToList();
        }
    }
}
