using Dapper;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using MoviesAPI.Models;
using MoviesAPI.Models.System;
using MoviesAPI.Repositories.Interface;
using Npgsql;
using System.Data.Common;
using System.Runtime;
using System.Transactions;

namespace MoviesAPI.Repositories.Implementation
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MovieRepository(IDbConnectionFactory connection)
        {
            _connectionFactory = connection;
        }

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            using var conn = _connectionFactory.CreateConnection();

            string sql = @"SELECT m.id, m.name, m.duration, m.release_date, m.amount, m.poster_path, m.plot, m.actors, m.directors,
                            COALESCE(string_agg(g.name, ', '), '') AS Genres
                           FROM movie m
                           LEFT JOIN moviegenres mg ON m.id = mg.movieid
                           LEFT JOIN genres g ON mg.genreid = g.id
                           GROUP BY m.id, m.name, m.duration, m.release_date, m.amount, m.poster_path, m.plot, m.actors, m.directors
                           ORDER BY m.id;";

            var movies = (await conn.QueryAsync<Movie>(sql)).ToList();


            return movies;
        }

        public async Task<Movie> GetByIdAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();


            string sql = @"SELECT m.id, m.name, m.duration, m.release_date, m.amount, m.poster_path, m.plot, m.actors, m.directors,
                             COALESCE(string_agg(g.name, ', '), '') AS Genres
                           FROM movie m
                           LEFT JOIN moviegenres mg ON m.id = mg.movieid
                           LEFT JOIN genres g ON mg.genreid = g.id
                           WHERE m.id = @id
                           GROUP BY m.id, m.name, m.duration, m.release_date, m.amount, m.poster_path, m.plot, m.actors, m.directors;";


            var movie = await conn.QueryFirstOrDefaultAsync<Movie>(sql, new { Id = id });

  

            return movie;
        }

        public async Task<CreateAndUpdateMovie> GetForUpdateAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = "SELECT name, duration, release_date, amount, poster_path, plot, actors, directors FROM movie WHERE id = @id";

            var updateMovie = await conn.QueryFirstOrDefaultAsync<CreateAndUpdateMovie>(sql, new { id });
            return updateMovie;

        }

        public async Task<int> CreateAsync(CreateAndUpdateMovie movie)
        {
            using var conn = _connectionFactory.CreateConnection();

            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string movieSql = @"
            INSERT INTO movie(name, duration, release_date, amount, poster_path, plot, actors, directors)
            VALUES(@Name, @Duration, @Release_Date, @Amount, @Poster_Path, @Plot, @Actors, @Directors)
            RETURNING id;";

                var movieId = await conn.ExecuteScalarAsync<int>(movieSql, movie, transaction);

                string genreIdSql = "SELECT id FROM genres WHERE name = ANY(@Names);";
                var genreIds = (await conn.QueryAsync<int>(genreIdSql, new { Names = movie.Genres }, transaction)).ToList();

                string genreSql = "INSERT INTO moviegenres(movieid, genreid) VALUES(@MovieId, @GenreId);";

                foreach (var genreId in genreIds)
                {
                    await conn.ExecuteAsync(genreSql, new { MovieId = movieId, GenreId = genreId }, transaction);
                }

                await transaction.CommitAsync();

                return movieId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }


        public async Task<int> UpdateAsync(long id, CreateAndUpdateMovie updateMovie)
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"UPDATE movie
                       SET name = @Name,
                           duration = @Duration,
                           release_date = @Release_Date,
                           amount = @Amount,
                           poster_path = @Poster_Path,
                           plot = @Plot,
                           actors = @Actors,
                           directors = @Directors
                       WHERE id = @Id";

                await conn.ExecuteAsync(sql, new
                {
                    Id = id,
                    updateMovie.Name,
                    updateMovie.Duration,
                    updateMovie.Release_Date,
                    updateMovie.Amount,
                    updateMovie.Poster_Path,
                    updateMovie.Plot,
                    updateMovie.Actors,
                    updateMovie.Directors
                }, transaction);

                string existingSql = "SELECT g.name FROM moviegenres mg JOIN genres g ON mg.genreid = g.id WHERE mg.movieid = @MovieId;";
                var existingGenres = (await conn.QueryAsync<string>(existingSql, new { MovieId = id }, transaction)).ToList();

                var newGenres = updateMovie.Genres ?? new List<string>();

                var genresToAdd = newGenres.Except(existingGenres).ToList();
                var genresToRemove = existingGenres.Except(newGenres).ToList();

                if (genresToRemove.Count > 0)
                {
                    string removeSql = @"DELETE FROM moviegenres 
                                 WHERE movieid = @MovieId AND genreid = ANY(
                                     SELECT id FROM genres WHERE name = ANY(@Names)
                                 );";
                    await conn.ExecuteAsync(removeSql, new { MovieId = id, Names = genresToRemove }, transaction);
                }

                if (genresToAdd.Count > 0)
                {
                    string genreIdSql = "SELECT id FROM genres WHERE name = ANY(@Names);";
                    var genreIds = (await conn.QueryAsync<int>(genreIdSql, new { Names = genresToAdd }, transaction)).ToList();

                    string insertSql = "INSERT INTO moviegenres(movieid, genreid) VALUES(@MovieId, @GenreId);";
                    foreach (var genreId in genreIds)
                    {
                        await conn.ExecuteAsync(insertSql, new { MovieId = id, GenreId = genreId }, transaction);
                    }
                }

                await transaction.CommitAsync();
                return 1;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<int> DeleteAsync(long id)
        {
            using var conn = _connectionFactory.CreateConnection();

            string sql = "delete from movie where id = @id";

            return await conn.ExecuteAsync(sql, new { id });

        }



        public async Task<IEnumerable<Movie>> GetByGenreAsync(string genre)
        {
            using var conn = _connectionFactory.CreateConnection();
            string sql = @"SELECT m.id, m.name, m.duration, m.release_date, m.amount, m.poster_path, m.plot, m.actors, m.directors,
                               COALESCE(string_agg(g.name, ', '), '') AS Genres
                           FROM movie m
                           LEFT JOIN moviegenres mg ON m.id = mg.movieid
                           LEFT JOIN genres g ON mg.genreid = g.id
                           WHERE m.id IN (
                              SELECT m2.id
                              FROM movie m2
                              LEFT JOIN moviegenres mg2 ON m2.id = mg2.movieid
                              LEFT JOIN genres g2 ON mg2.genreid = g2.id
                              WHERE LOWER(g2.name) = LOWER(@GenreName)
                           )
                           GROUP BY m.id, m.name, m.duration, m.release_date, m.amount, m.poster_path, m.plot, m.actors, m.directors
                           ORDER BY m.id;";

            var movies = await conn.QueryAsync<Movie>(sql, new { GenreName = genre });


            return movies;
        }

        public async Task<List<Movie>> GetTopNAsync(int n)
        {
            using var conn = _connectionFactory.CreateConnection();

            string sql = @"
                WITH movie_stats AS (
                    SELECT 
                        m.*,
                        COUNT(r.rating) AS rating_count,
                        AVG(r.rating) AS avg_rating
                    FROM movie m
                    LEFT JOIN movieratings r ON m.id = r.movie_id
                    GROUP BY m.id
                ),
                global_avg AS (
                    SELECT AVG(rating) AS C FROM movieratings
                ),
                user_count AS (
                    SELECT COUNT(*) AS total_users FROM users
                )
                SELECT 
                    ms.*,
                    ((ms.rating_count::float / (ms.rating_count + uc.total_users * 0.55)) * ms.avg_rating
                     + ((uc.total_users * 0.55) / (ms.rating_count + uc.total_users * 0.55)) * ga.C) AS weighted_rating
                FROM movie_stats ms
                CROSS JOIN global_avg ga
                CROSS JOIN user_count uc
                ORDER BY weighted_rating DESC
                LIMIT @Limit;
            ";

            var movies = (await conn.QueryAsync<Movie>(sql, new { Limit = n })).ToList();

         
            return movies;
        }


      


       


        

    }

}
