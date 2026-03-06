using MoviesAPI.Models;

namespace MoviesAPI.Repositories.Interface
{
    public interface IRatingRepository
    {
        Task<(decimal WeightedRating, List<MovieRating> Ratings)> GetForMovieAsync(long movieId);
        Task<MovieRating?> GetForUserAndMovieAsync(long movieId, long userId);
        Task<bool> UpsertAsync(CreateRating rating);
    }
}
