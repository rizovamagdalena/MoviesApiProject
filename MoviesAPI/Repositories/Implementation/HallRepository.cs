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
        private readonly DBSettings _dbSettings;

        public HallRepository(IOptions<DBSettings> dbSettings)
        {
            _dbSettings = dbSettings.Value;
        }

        public async Task<List<HallDto>> GetAllHallsAsync()
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            string sql = @"
                    SELECT 
                        h.id AS Id, 
                        h.name AS Name,
                        h.rows As Rows,
                        h.seats_per_row AS Seats_Per_Row,
                        hs.id AS SeatId, 
                        hs.row_number AS RowNumber, 
                        hs.seat_number AS SeatNumber
                    FROM hall h
                    LEFT JOIN hall_seat hs ON hs.hall_id = h.id
                    ORDER BY h.id, hs.row_number, hs.seat_number;
    ";

            var hallDictionary = new Dictionary<int, HallDto>();

            var result = await conn.QueryAsync<HallDto, HallSeatDto, HallDto>(
                sql,
                (hall, seat) =>
                {
                    if (!hallDictionary.TryGetValue(hall.Id, out var hallEntry))
                    {
                        hallEntry = new HallDto
                        {
                            Id = hall.Id,
                            Name = hall.Name,
                            Rows = hall.Rows,
                            Seats_Per_Row = hall.Seats_Per_Row,
                            Seats = new List<HallSeatDto>(),
                        };
                        hallDictionary.Add(hall.Id, hallEntry);
                    }

                    if (seat != null)
                        hallEntry.Seats.Add(seat);

                    return hallEntry;
                },
                splitOn: "SeatId"
            );

            return hallDictionary.Values.ToList();
        }

      

        public async Task<HallDto> GetHallByIdAsync(int hallId)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = @"
                SELECT h.id, h.name,
                       hs.id AS SeatId, hs.row_number AS RowNumber, hs.seat_number AS SeatNumber
                FROM hall h
                LEFT JOIN hall_seat hs ON hs.hall_id = h.id
                WHERE h.id = @HallId
                ORDER BY hs.row_number, hs.seat_number;
            ";


            var hallDictionary = new Dictionary<int, HallDto>();

            var result = await conn.QueryAsync<HallDto, HallSeatDto, HallDto>(
                sql,
                (hall, seat) =>
                {
                    if (!hallDictionary.TryGetValue(hall.Id, out var hallEntry))
                    {
                        hallEntry = new HallDto
                        {
                            Id = hall.Id,
                            Name = hall.Name,
                            Seats = new List<HallSeatDto>()
                        };
                        hallDictionary.Add(hall.Id, hallEntry);
                    }

                    if (seat != null)
                        hallEntry.Seats.Add(seat);

                    return hallEntry;
                },
                new { HallId = hallId },
                splitOn: "SeatId"
            );

            return hallDictionary.Values.FirstOrDefault();
        }

        public async Task<List<HallSeatDto>> GetSeatsByHallIdAsync(int hallId)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = @"
                SELECT id AS SeatId, hall_id AS HallId, row_number AS RowNumber,seat_number AS SeatNumber
                FROM hall_seat
                WHERE hall_id = @HallId
                ORDER BY RowNumber, SeatNumber;
            ";

            var seats = await conn.QueryAsync<HallSeatDto>(sql, new { HallId = hallId });
            return seats.ToList();
        }
    }
}
