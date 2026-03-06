using MoviesAPI.Models;

namespace MoviesAPI.Service.Interface
{
    public interface IFutureMovieService
    {
        Task<IEnumerable<FutureMovie>> GetAllAsync();
        Task<FutureMovie?> GetByIdAsync(long id);
        Task<int> CreateAsync(CreateFutureMovie movie);
        Task<int> DeleteAsync(long id);
    }
}
