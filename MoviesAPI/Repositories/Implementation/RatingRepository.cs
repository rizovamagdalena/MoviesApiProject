using Dapper;
using Microsoft.AspNetCore.Connections;
using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;

namespace MoviesAPI.Repositories.Implementation
{
    public class RatingRepository : IRatingRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public RatingRepository(IDbConnectionFactory connection)
        {
            _connectionFactory = connection;
        }

        public async Task<(decimal WeightedRating, List<MovieRating> Ratings)> GetForMovieAsync(long movieId)
        {
            using var conn = _connectionFactory.CreateConnection();

            string ratingsSql = @"
                SELECT r.id AS Id, r.user_id AS UserId, r.movie_id AS MovieId, r.rating AS Rating, r.comment AS Comment,
                       u.username AS UserName
                FROM movieratings r
                JOIN users u ON r.user_id = u.id
                WHERE r.movie_id = @MovieId;
            ";
            var ratings = (await conn.QueryAsync<MovieRating>(ratingsSql, new { MovieId = movieId })).ToList();




            string weightedSql = @"
                WITH movie_stats AS (
                    SELECT 
                        COUNT(r.rating) AS rating_count,
                        AVG(r.rating) AS avg_rating
                    FROM movieratings r
                    WHERE r.movie_id = @MovieId
                ),
                global_avg AS (
                    SELECT AVG(rating) AS C FROM movieratings
                ),
                user_count AS (
                    SELECT COUNT(*) AS total_users FROM users
                )
                SELECT 
                    ((ms.rating_count::float / (ms.rating_count + uc.total_users * 0.55)) * ms.avg_rating
                     + ((uc.total_users * 0.55) / (ms.rating_count + uc.total_users * 0.55)) * ga.C) AS weighted_rating
                FROM movie_stats ms
                CROSS JOIN global_avg ga
                CROSS JOIN user_count uc;
            ";

            var weightedRating = await conn.ExecuteScalarAsync<decimal?>(weightedSql, new { MovieId = movieId }) ?? 0;


            return (weightedRating, ratings);

        }


        public async Task<MovieRating?> GetForUserAndMovieAsync(long movieId, long userId)
        {

            using var conn = _connectionFactory.CreateConnection();

            string sql = @"
                SELECT r.id AS Id, r.user_id AS UserId, r.movie_id AS MovieId, r.rating AS Rating, r.comment AS Comment,
                       u.username AS UserName
                FROM movieratings r
                JOIN users u ON r.user_id = u.id
                WHERE r.movie_id = @MovieId AND r.user_id = @UserId;
            ";


            var result = await conn.QueryFirstOrDefaultAsync<MovieRating>(sql, new { MovieId = movieId, UserId = userId });

            return result;

        }



        public async Task<bool> UpsertAsync(CreateRating rating)
        {
            using var conn = _connectionFactory.CreateConnection();

            string sql = @"
                INSERT INTO movieratings (movie_id, user_id, rating,comment)
                VALUES (@MovieId, @UserId, @Rating,@Comment)
                ON CONFLICT (movie_id, user_id) 
                DO UPDATE SET rating = EXCLUDED.rating,
                              comment = EXCLUDED.comment;
            ";

            var rows = await conn.ExecuteAsync(sql, new
            {
                MovieId = rating.MovieId,
                UserId = rating.UserId,
                Rating = rating.Rating,
                Comment = rating.Comment
            });
            return rows > 0;
        }

    }
}
