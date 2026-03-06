using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;

namespace MoviesAPI.Service.Implementation
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;

        public RatingService(IRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }

        public async Task<MovieRating?> GetRatingOfUserForMovieAsync(long movieId, long userId)
        {
            return await _ratingRepository.GetForUserAndMovieAsync(movieId, userId);
        }

        public async Task<bool> UpsertRatingAsync(CreateRating rating)
        {
            if (rating.Rating < 1 || rating.Rating > 10)
                throw new ArgumentOutOfRangeException(nameof(rating.Rating), "Rating must be between 1 and 10.");

            return await _ratingRepository.UpsertAsync(rating);
        }
    }
}
