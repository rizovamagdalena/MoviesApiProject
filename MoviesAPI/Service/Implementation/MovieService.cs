using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;

namespace MoviesAPI.Service.Implementation
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IRatingRepository _ratingRepository;

        public MovieService(IMovieRepository movieRepository, IRatingRepository ratingRepository)
        {
            _movieRepository = movieRepository;
            _ratingRepository = ratingRepository;
        }

        private async Task EnrichWithRatingsAsync(IEnumerable<Movie> movies)
        {
            foreach (var movie in movies)
            {
                (movie.Rating, movie.Ratings) = await _ratingRepository.GetForMovieAsync(movie.Id);
            }
        }

        public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
        {
            var movies = await _movieRepository.GetAllAsync();
            await EnrichWithRatingsAsync(movies);
            return movies;
        }

        public async Task<Movie?> GetMovieByIdAsync(long id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null) return null;

            (movie.Rating, movie.Ratings) = await _ratingRepository.GetForMovieAsync(movie.Id);
            return movie;
        }

        public async Task<int> CreateMovieAsync(CreateAndUpdateMovie movie)
        {
            return await _movieRepository.CreateAsync(movie);
        }

        public async Task<int> UpdateMovieAsync(long id, CreateAndUpdateMovie movie)
        {
            return await _movieRepository.UpdateAsync(id, movie);
        }

        public async Task<int> DeleteMovieAsync(long id)
        {
            return await _movieRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Movie>> GetByGenreAsync(string genre)
        {
            var movies = await _movieRepository.GetByGenreAsync(genre);
            await EnrichWithRatingsAsync(movies);
            return movies;
        }

        public async Task<List<Movie>> GetTopRatedAsync(int n)
        {
            var movies = await _movieRepository.GetTopNAsync(n);
            await EnrichWithRatingsAsync(movies);
            return movies;
        }
    }
}
