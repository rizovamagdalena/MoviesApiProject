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
        private readonly IDbConnectionFactory _connectionFactory;

        private const string BaseSelect = @"
            SELECT id, name, phone, username, password, email, active, role 
            FROM users";

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<User>(BaseSelect);
        }

        public async Task<User?> GetByIdAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<User>(
                $"{BaseSelect} WHERE id = @Id;", new { Id = id });
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<User>(
                $"{BaseSelect} WHERE username = @Username;", new { Username = username });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = @"
                SELECT id, name, phone, username, password, email, active AS ""IsActive"", role
                FROM users WHERE email = @Email;";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<UserProfile?> GetForUpdateAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<UserProfile>(
                "SELECT name, phone, username FROM users WHERE id = @Id;", new { Id = id });
        }

        public async Task<long?> GetUserIdByUsernameAsync(string username)
        {
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<long?>(
                "SELECT id FROM users WHERE username = @Username;", new { Username = username });
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            using var conn = _connectionFactory.CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM users WHERE email = @Email;", new { Email = email });
            return count > 0;
        }

        public async Task<User?> GetByUsernameAndPasswordAsync(string username, string password)
        {
            using var conn = _connectionFactory.CreateConnection();

            string sql = @"
                SELECT id, name, phone, username, password, active AS ""IsActive"", role
                FROM users
                WHERE username = @Username AND password = @Password;";

            var user = await conn.QueryFirstOrDefaultAsync<User>(sql, new { Username = username, Password = password });

            if (user == null) return null;

            await conn.ExecuteAsync(
                "UPDATE users SET active = true WHERE username = @Username;",
                new { Username = username });

            return user;
        }

        public async Task<bool> LogoutAsync(string username)
        {
            using var conn = _connectionFactory.CreateConnection();
            var rows = await conn.ExecuteAsync(
                "UPDATE users SET active = false WHERE username = @Username;",
                new { Username = username });
            return rows > 0;
        }

        public async Task<int> CreateAsync(RegisterRequest user)
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = @"
                INSERT INTO users(name, phone, username, password, email, active, emailconfirmed)
                VALUES(@Name, @Phone, @Username, @Password, @Email, @IsActive, @EmailConfirmed)
                RETURNING id;";
            return await conn.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<int> UpdateAsync(UserProfile updateUser)
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = @"
                UPDATE users SET name = @Name, phone = @Phone
                WHERE username = @Username;";
            return await conn.ExecuteAsync(sql, new
            {
                updateUser.Name,
                updateUser.Phone,
                updateUser.Username
            });
        }

        public async Task<int> DeleteAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM users WHERE id = @Id;", new { Id = id });
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            using var conn = _connectionFactory.CreateConnection();
            var rows = await conn.ExecuteAsync(
                "UPDATE users SET password = @Password WHERE id = @UserId;",
                new { Password = newPassword, UserId = userId });
            return rows > 0;
        }
    }
}
