using Dapper;
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
        private readonly DBSettings _dbSettings;

        public MovieRepository(IOptions<DBSettings> dbSettings)
        {
            _dbSettings = dbSettings.Value;
        }

       public async Task<IEnumerable<Movie>> GetMoviesAsync()
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            string sql = @"SELECT m.id, m.name, m.duration, m.release_date, m.amount, m.poster_path, m.plot, m.actors, m.directors,
                                  string_agg(g.name, ', ') AS Genres
                           FROM movie m
                           INNER JOIN moviegenres mg ON m.id = mg.movieid
                           INNER JOIN genres g ON mg.genreid = g.id
                           GROUP BY m.id, m.name, m.duration, m.release_date, m.amount, m.poster_path, m.plot, m.actors, m.directors
                           ORDER BY m.id;";

            var movies = await conn.QueryAsync<Movie>(sql);

            foreach (var movie in movies)
            {
                movie.Rating = await GetRatingOfAMovieAsync(movie.Id) ?? 0;
            }

            return movies;
        }

        public async Task<IEnumerable<Movie>> GetMoviesByGenreAsync(string genre)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            string sql = @"SELECT m.id, m.name, m.duration, m.release_date, m.amount, m.poster_path, m.plot, m.actors, m.directors,
                                  string_agg(g.name, ', ') AS Genres
                           FROM movie m
                           INNER JOIN moviegenres mg ON m.id = mg.movieid
                           INNER JOIN genres g ON mg.genreid = g.id
                           WHERE m.id IN (
                              SELECT m2.id
                              FROM movie m2
                              INNER JOIN moviegenres mg2 ON m2.id = mg2.movieid
                              INNER JOIN genres g2 ON mg2.genreid = g2.id
                              WHERE LOWER(g2.name) = LOWER(@GenreName)
                           )
                           GROUP BY m.id, m.name, m.duration, m.release_date, m.amount, m.poster_path, m.plot, m.actors, m.directors
                           ORDER BY m.id;";

            var movies = await conn.QueryAsync<Movie>(sql, new { GenreName = genre });

            foreach (var movie in movies)
            {
                movie.Rating = await GetRatingOfAMovieAsync(movie.Id) ?? 0;

            }

            return movies;
        }

        public async Task<Movie> GetMovieAsync(long id)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
           

            string sql = @"SELECT m.id, m.name, m.duration, m.release_date, m.amount, m.poster_path, m.plot, m.actors, m.directors,
                                  string_agg(g.name, ', ') AS Genres
                           FROM movie m
                           INNER JOIN moviegenres mg ON m.id = mg.movieid
                           INNER JOIN genres g ON mg.genreid = g.id
                           WHERE m.id = @id
                           GROUP BY m.id, m.name, m.duration, m.release_date, m.amount, m.poster_path, m.plot, m.actors, m.directors
                           ORDER BY m.id;";


            var movie = await conn.QueryFirstOrDefaultAsync<Movie>(sql, new { Id=id });

            if (movie != null)
            {
                movie.Rating = await GetRatingOfAMovieAsync(movie.Id) ?? 0;
            }

            return movie;
        }

        public async Task<CreateAndUpdateMovie> GetMovieForUpdateAsync(long id)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            string sql = "SELECT name, duration, release_date, amount, poster_path, plot, actors, directors FROM movie WHERE id = @id";

            var updateMovie = await conn.QueryFirstOrDefaultAsync<CreateAndUpdateMovie>(sql, new { id });
            return updateMovie;

        }

        public async Task<int> CreateMovieAsync(CreateAndUpdateMovie movie)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

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

        public async Task<int> UpdateMovieAsync(long id, CreateAndUpdateMovie updateMovie)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
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

        public async Task<int> DeleteMovieAsync(long id)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = "delete from movie where id = @id";

            return await conn.ExecuteAsync(sql, new { id });

        }

        public async Task<IEnumerable<FutureMovie>> GetFutureMoviesAsync()
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = "SELECT id, name, genres, poster_path FROM futuremovies ORDER BY id;";


            var result = await conn.QueryAsync<FutureMovie>(sql);

            return result;

        }

        public async Task<FutureMovie> GetFutureMovieAsync(long id)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = "SELECT id, name, genres, poster_path FROM futuremovies WHERE id = @Id;";


            var result = await conn.QueryFirstOrDefaultAsync<FutureMovie>(sql, new { Id= id } );

            return result;
        }

        public async Task<int> CreateFutureMovieAsync(CreateFutureMovie movie)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

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

     

        public async Task<int> DeleteFutureMovieAsync(long id)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = "delete from futuremovies where id = @id";

            return await conn.ExecuteAsync(sql, new { id });
        }

        public async Task<List<string>> GettAllGenresAsync()
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);
            string sql = "SELECT name FROM genres ";

            var genres = await conn.QueryAsync<string>(sql);
            return genres.ToList();
        }

        public async Task<double?> GetRatingOfAMovieAsync(long movieId)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = @"
                WITH movie_stats AS (
                    SELECT 
                        m.id,
                        COUNT(r.rating) AS rating_count,
                        AVG(r.rating) AS avg_rating
                    FROM movie m
                    LEFT JOIN movieratings r ON m.id = r.movie_id
                    WHERE m.id = @MovieId
                    GROUP BY m.id
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

            var weightedRating = await conn.ExecuteScalarAsync<double?>(sql, new { MovieId = movieId });

            return weightedRating;
        }


        public async Task<int?> GetRatingOfUserForMovieAsync(long movieId, long userId)
        {

            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

            string sql = @"
                SELECT rating 
                FROM movieratings
                WHERE movie_id = @MovieId AND user_id = @UserId;
            ";


            var result = await conn.ExecuteScalarAsync<int>(sql, new { MovieId = movieId,  UserId=userId });

            return result;

        }



        public async Task<bool> UpsertRating(long movieId, long userId, int rating)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

                    string sql = @"
                INSERT INTO movieratings (movie_id, user_id, rating)
                VALUES (@MovieId, @UserId, @Rating)
                ON CONFLICT (movie_id, user_id) 
                DO UPDATE SET rating = EXCLUDED.rating;
            ";

            var rows = await conn.ExecuteAsync(sql, new { MovieId = movieId, UserId = userId, Rating = rating });
            return rows > 0;
        }



        public async Task<List<Movie>> GetTopNMoviesAsync(int n)
        {
            using var conn = new NpgsqlConnection(_dbSettings.PostgresDB);

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

            foreach (var movie in movies)
            {
                movie.Rating = await GetRatingOfAMovieAsync(movie.Id) ?? 0;
            }

            return movies;
        }

    }

}
