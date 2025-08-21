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

            var result = await conn.QueryAsync<Movie>(sql);
            return result;
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

            var result = await conn.QueryAsync<Movie>(sql, new { GenreName = genre });
            return result;
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

        public async Task<int> CreateFutureMovieAsync(FutureMovie movie)
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
    }

}
