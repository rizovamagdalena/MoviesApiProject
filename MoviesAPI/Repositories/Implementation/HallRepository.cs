using Dapper;
using Microsoft.Extensions.Options;
using MoviesAPI.Models;
using MoviesAPI.Models.System;
using MoviesAPI.Repositories.Interface;
using Npgsql;

namespace MoviesAPI.Repositories.Implementation
{
    public class HallRepository : IHallRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public HallRepository(IDbConnectionFactory connection)
        {
            _connectionFactory = connection;
        }


        public async Task<List<HallDto>> GetAllAsync()
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = @"
                SELECT h.id AS Id, h.name AS Name, h.rows AS Rows,
                       h.seats_per_row AS Seats_Per_Row,
                       hs.id AS SeatId, hs.row_number AS RowNumber, hs.seat_number AS SeatNumber
                FROM hall h
                LEFT JOIN hall_seat hs ON hs.hall_id = h.id
                ORDER BY h.id, hs.row_number, hs.seat_number;";

            var hallDictionary = new Dictionary<int, HallDto>();

            await conn.QueryAsync<HallDto, HallSeatDto, HallDto>(sql, (hall, seat) =>
            {
                if (!hallDictionary.TryGetValue(hall.Id, out var entry))
                {
                    entry = new HallDto
                    {
                        Id = hall.Id,
                        Name = hall.Name,
                        Rows = hall.Rows,
                        Seats_Per_Row = hall.Seats_Per_Row,
                        Seats = new List<HallSeatDto>()
                    };
                    hallDictionary.Add(hall.Id, entry);
                }

                if (seat != null) entry.Seats.Add(seat);
                return entry;
            }, splitOn: "SeatId");

            return hallDictionary.Values.ToList();
        }

        public async Task<HallDto?> GetByIdAsync(int hallId)
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = @"
                SELECT h.id AS Id, h.name AS Name, h.rows AS Rows,
                       h.seats_per_row AS Seats_Per_Row,
                       hs.id AS SeatId, hs.row_number AS RowNumber, hs.seat_number AS SeatNumber
                FROM hall h
                LEFT JOIN hall_seat hs ON hs.hall_id = h.id
                WHERE h.id = @HallId
                ORDER BY hs.row_number, hs.seat_number;";

            var hallDictionary = new Dictionary<int, HallDto>();

            await conn.QueryAsync<HallDto, HallSeatDto, HallDto>(sql, (hall, seat) =>
            {
                if (!hallDictionary.TryGetValue(hall.Id, out var entry))
                {
                    entry = new HallDto
                    {
                        Id = hall.Id,
                        Name = hall.Name,
                        Rows = hall.Rows,
                        Seats_Per_Row = hall.Seats_Per_Row,
                        Seats = new List<HallSeatDto>()
                    };
                    hallDictionary.Add(hall.Id, entry);
                }

                if (seat != null) entry.Seats.Add(seat);
                return entry;
            }, new { HallId = hallId }, splitOn: "SeatId");

            return hallDictionary.Values.FirstOrDefault();
        }

        public async Task<List<HallSeatDto>> GetSeatsByHallIdAsync(int hallId)
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = @"
                SELECT id AS SeatId, hall_id AS HallId,
                       row_number AS RowNumber, seat_number AS SeatNumber
                FROM hall_seat
                WHERE hall_id = @HallId
                ORDER BY RowNumber, SeatNumber;";

            return (await conn.QueryAsync<HallSeatDto>(sql, new { HallId = hallId })).ToList();
        }
    }
}
