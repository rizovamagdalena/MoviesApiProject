using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoviesAPI.Models.System;
using MoviesAPI.Repositories.Interface;
using Npgsql;

namespace MoviesAPI.Repositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly DBSettings _dbSettings;

        public UserRepository(IOptions<DBSettings> dbSettings)
        {
            _dbSettings = dbSettings.Value;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            string sql = "SELECT id, name, phone, username, password,active,role FROM users";

            var result = await conn.QueryAsync<User>(sql);
            return result;
        }

        public async Task<User> GetUserAsync(long id)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            string sql = "SELECT id, name, phone, username, password,active,role FROM users WHERE id = @id";

            var user = await conn.QueryFirstOrDefaultAsync<User>(sql, new { id });
            return user;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            string sql = "SELECT id, name, phone, username, password,active,role FROM users WHERE username = @username";

            var user = await conn.QueryFirstOrDefaultAsync<User>(sql, new { username });
            return user;
        }

        public async Task<UserProfile> GetUserForUpdateAsync(long id)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            string sql = "SELECT name, phone, username FROM users WHERE id = @id";

            var updateUser = await conn.QueryFirstOrDefaultAsync<UserProfile>(sql, new { id });
            return updateUser;

        }

        public async Task<int> CreateUserAsync(RegisterRequest user)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = "INSERT into users(name, phone, username, password,active) VALUES(@Name, @Phone, @Username, @Password, @isActive) RETURNING id;";

            var id = await conn.ExecuteScalarAsync<int>(sql, user);
            return id;

        }

        public async Task<int> UpdateUserAsync( UserProfile updateUser)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = @"UPDATE users
                   SET name = @Name,
                       phone = @Phone
                   WHERE username = @Username";

            return await conn.ExecuteAsync(sql, new
            {
                updateUser.Name,
                updateUser.Phone,
                updateUser.Username
            });
        }

        public async Task<int> DeleteUserAsync(long id)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = "delete from users where id = @id";

            return await conn.ExecuteAsync(sql, new { id });

        }

        public async Task<User> GetUserByUsernameAndPassword(string username, string password)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = @"SELECT id, name, phone, username, password,active AS ""IsActive"", role
                         FROM users
                         WHERE username = @username AND password = @password";

            var user = await conn.QueryFirstOrDefaultAsync<User>(sql, new { username, password });

            if (user == null)
                return null;

            string updateSql = @"UPDATE users SET active = true WHERE username = @username";
            await conn.ExecuteAsync(updateSql, new { username });


            return user;
        }

        public async Task<bool> LogoutUser(string username)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = @"UPDATE users SET active = false WHERE username = @username";
            int rowsAffected = await conn.ExecuteAsync(sql, new { username });

            return rowsAffected > 0;
        }
    }
}
