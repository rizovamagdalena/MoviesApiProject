using MoviesAPI.Models;

namespace MoviesAPI.Repositories.Interface
{
    public interface IFutureMovieRepository
    {
        Task<IEnumerable<FutureMovie>> GetAllAsync();
        Task<FutureMovie?> GetByIdAsync(long id);
        Task<int> CreateAsync(CreateFutureMovie movie);
        Task<int> DeleteAsync(long id);
    }
}
