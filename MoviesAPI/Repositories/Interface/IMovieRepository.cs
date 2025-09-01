using MoviesAPI.Models;

namespace MoviesAPI.Repositories.Interface
{
    public interface IMovieRepository
    {
        Task<IEnumerable<Movie>> GetMoviesAsync();
        Task<IEnumerable<Movie>> GetMoviesByGenreAsync(string genre);
        Task< Movie> GetMovieAsync(long id);
        Task<int> CreateMovieAsync(CreateAndUpdateMovie movie);
        Task<int> UpdateMovieAsync(long id, CreateAndUpdateMovie movie);
        Task<CreateAndUpdateMovie> GetMovieForUpdateAsync(long id);
        Task<int> DeleteMovieAsync(long id);
        Task<IEnumerable<FutureMovie>> GetFutureMoviesAsync();
        Task<int> CreateFutureMovieAsync(CreateFutureMovie movie);
        Task<FutureMovie> GetFutureMovieAsync(long id);
        Task<int> DeleteFutureMovieAsync(long id);
        Task<List<string>> GettAllGenresAsync();
        Task<(decimal WeightedRating, List<MovieRating> Ratings)> GetRatingsForMovieAsync(long movieId);
        Task<MovieRating?> GetRatingOfUserForMovieAsync(long movieId, long userId);
        Task<List<Movie>> GetTopNMoviesAsync(int n);
        Task<bool> UpsertRating(CreateRating rating);

    }
}
