using Npgsql;

namespace MoviesAPI.Repositories
{
    public interface IDbConnectionFactory
    {
        NpgsqlConnection CreateConnection();
    }
}
