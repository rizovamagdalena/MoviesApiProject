using Dapper;
using MoviesAPI.Models;
using MoviesAPI.Models.System;
using MoviesAPI.Repositories.Interface;
using Microsoft.Extensions.Options;
using Npgsql;

namespace MoviesAPI.Repositories.Implementation
{
    public class ChatBotRepository : IChatBotRepository
    {
        private readonly DBSettings _dbSettings;

        public ChatBotRepository(IOptions<DBSettings> dbSettings)
        {
            _dbSettings = dbSettings.Value;
        }

        private NpgsqlConnection GetConnection() => new NpgsqlConnection(_dbSettings.PostgresDB);

        public async Task<List<Faq>> GetAllFaqAsync()
        {
            using var conn = GetConnection();
            var sql = "SELECT * FROM faqs ORDER BY createdat DESC";
            var faqs = (await conn.QueryAsync<Faq>(sql)).ToList();
            return faqs;
        }

        public async Task<Faq?> GetFaqByIdAsync(int id)
        {
            using var conn = GetConnection();
            var sql = "SELECT * FROM faqs WHERE id = @Id";
            return await conn.QueryFirstOrDefaultAsync<Faq>(sql, new { Id = id });
        }

        public async Task AddFaqAsync(Faq faq)
        {
            using var conn = GetConnection();
            var sql = @"INSERT INTO faqs (question, answer, category, createdat, updatedAt)
                        VALUES (@Question, @Answer, @Category, NOW(), NOW())
                        RETURNING id;";
            faq.Id = await conn.ExecuteScalarAsync<int>(sql, faq);
        }

        public async Task UpdateFaqAsync(Faq faq)
        {
            using var conn = GetConnection();
            var sql = @"UPDATE faqs
                        SET question = @Question,
                            answer = @Answer,
                            category = @Category,
                            updatedat = NOW()
                        WHERE id = @Id";
            await conn.ExecuteAsync(sql, faq);
        }

        public async Task DeleteFaqAsync(int id)
        {
            using var conn = GetConnection();
            var sql = "DELETE FROM faqs WHERE id = @Id";
            await conn.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<Faq?> GetClosestMatchToFaqAsync(string userQuestion)
        {
            var faqs = await GetAllFaqAsync();
            if (string.IsNullOrWhiteSpace(userQuestion)) return null;

            userQuestion = userQuestion.ToLower();

            foreach (var faq in faqs)
            {
                if (faq.Question.ToLower().Contains(userQuestion))
                    return faq;
            }

            return null;
        }
    }
}
