using Microsoft.Extensions.Options;
using MoviesAPI.Models.System;
using Npgsql;

namespace MoviesAPI.Repositories
{

    public class NpgsqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public NpgsqlConnectionFactory(IOptions<DBSettings> dbSettings)
        {
            _connectionString = dbSettings.Value.PostgresDB;
        }

        public NpgsqlConnection CreateConnection()
            => new NpgsqlConnection(_connectionString);
    }
}
