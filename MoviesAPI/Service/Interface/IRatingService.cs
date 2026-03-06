using MoviesAPI.Models;

namespace MoviesAPI.Service.Interface
{
    public interface IRatingService
    {
        Task<MovieRating?> GetRatingOfUserForMovieAsync(long movieId, long userId);
        Task<bool> UpsertRatingAsync(CreateRating rating);
    }
}
