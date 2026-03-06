using Dapper;
using Microsoft.AspNetCore.Connections;
using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;

namespace MoviesAPI.Repositories.Implementation
{
    public class FutureMovieRepository : IFutureMovieRepository
    {

        private readonly IDbConnectionFactory _connectionFactory;

        public FutureMovieRepository(IDbConnectionFactory connection)
        {
            _connectionFactory = connection;
        }

        public async Task<IEnumerable<FutureMovie>> GetAllAsync()
        {
            using var conn = _connectionFactory.CreateConnection();

            string sql = "SELECT id, name, genres, poster_path FROM futuremovies ORDER BY id;";


            var result = await conn.QueryAsync<FutureMovie>(sql);

            return result;

        }

        public async Task<FutureMovie> GetByIdAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();

            string sql = "SELECT id, name, genres, poster_path FROM futuremovies WHERE id = @Id;";


            var result = await conn.QueryFirstOrDefaultAsync<FutureMovie>(sql, new { Id = id });

            return result;
        }

        public async Task<int> CreateAsync(CreateFutureMovie movie)
        {
            using var conn = _connectionFactory.CreateConnection();

            string sql = @"
            INSERT INTO futuremovies (name, genres, poster_path)
            VALUES(@Name,@Genres, @Poster_Path)
            RETURNING id;";

            var movieId = await conn.ExecuteScalarAsync<int>(sql, new
            {
                movie.Name,
                movie.Genres,
                movie.Poster_Path
            });

            return movieId;
        }



        public async Task<int> DeleteAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();

            string sql = "delete from futuremovies where id = @id";

            return await conn.ExecuteAsync(sql, new { id });
        }
    }
}
