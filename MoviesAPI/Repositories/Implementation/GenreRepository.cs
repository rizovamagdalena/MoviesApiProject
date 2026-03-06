using Dapper;
using Microsoft.AspNetCore.Connections;
using MoviesAPI.Repositories.Interface;

namespace MoviesAPI.Repositories.Implementation
{
    public class GenreRepository : IGenreRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public GenreRepository(IDbConnectionFactory connection)
        {
            _connectionFactory = connection;
        }

        public async Task<List<string>> GetAllAsync()
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = "SELECT name FROM genres ";

            var genres = await conn.QueryAsync<string>(sql);
            return genres.ToList();
        }

    }
}
