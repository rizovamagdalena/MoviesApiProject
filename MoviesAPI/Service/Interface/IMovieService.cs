using MoviesAPI.Models;

namespace MoviesAPI.Service.Interface
{
    public interface IMovieService
    {
        Task<IEnumerable<Movie>> GetAllMoviesAsync();
        Task<Movie?> GetMovieByIdAsync(long id);
        Task<int> CreateMovieAsync(CreateAndUpdateMovie movie);
        Task<int> UpdateMovieAsync(long id, CreateAndUpdateMovie movie);
        Task<int> DeleteMovieAsync(long id);
        Task<IEnumerable<Movie>> GetByGenreAsync(string genre);
        Task<List<Movie>> GetTopRatedAsync(int n);
    }

}
