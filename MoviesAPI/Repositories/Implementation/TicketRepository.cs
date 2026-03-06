using Dapper;
using Microsoft.Extensions.Options;
using MoviesAPI.Models;
using MoviesAPI.Models.System;
using MoviesAPI.Repositories.Interface;
using Npgsql;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MoviesAPI.Repositories.Implementation
{
    public class TicketRepository : ITicketRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public TicketRepository(IDbConnectionFactory connection)
        {
            _connectionFactory = connection;
        }

        public async Task<IEnumerable<TicketResponse>> GetAllAsync()
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = @"
                SELECT t.id, m.name AS moviename, m.poster_path AS posterpath,
                       u.username AS username, t.watch_movie, t.price,
                       h.id AS hallname, s.row_number AS row, s.seat_number AS column
                FROM ticket t
                INNER JOIN movie m ON t.movie_id = m.id
                INNER JOIN users u ON t.user_id = u.id
                INNER JOIN hall_seat s ON t.hall_seat_id = s.id
                INNER JOIN hall h ON s.hall_id = h.id;";

            return await conn.QueryAsync<TicketResponse>(sql);
        }

        public async Task<TicketResponse?> GetByIdAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = @"
                SELECT t.id, m.name AS moviename, m.poster_path AS posterpath,
                       u.username AS username, t.watch_movie, t.price,
                       h.id AS hallname, s.row_number AS row, s.seat_number AS column
                FROM ticket t
                INNER JOIN movie m ON t.movie_id = m.id
                INNER JOIN users u ON t.user_id = u.id
                INNER JOIN hall_seat s ON t.hall_seat_id = s.id
                INNER JOIN hall h ON s.hall_id = h.id
                WHERE t.id = @Id;";

            return await conn.QueryFirstOrDefaultAsync<TicketResponse>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(long movieId, long userId, DateTime watchMovie,
            decimal price, int hallSeatId, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            string sql = @"
                INSERT INTO ticket(movie_id, user_id, watch_movie, price, hall_seat_id)
                VALUES(@MovieId, @UserId, @WatchMovie, @Price, @HallSeatId)
                RETURNING id;";

            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                MovieId = movieId,
                UserId = userId,
                WatchMovie = watchMovie,
                Price = price,
                HallSeatId = hallSeatId
            }, transaction);
        }

        public async Task<int> DeleteAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync("DELETE FROM ticket WHERE id = @Id;", new { Id = id });
        }




    }
}
