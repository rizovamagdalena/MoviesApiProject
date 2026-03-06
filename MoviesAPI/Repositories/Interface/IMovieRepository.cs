using MoviesAPI.Models;

namespace MoviesAPI.Repositories.Interface
{
    public interface IMovieRepository
    {
      
        Task<IEnumerable<Movie>> GetAllAsync();
        Task<Movie?> GetByIdAsync(long id);
        Task<CreateAndUpdateMovie?> GetForUpdateAsync(long id);
        Task<int> CreateAsync(CreateAndUpdateMovie movie);
        Task<int> UpdateAsync(long id, CreateAndUpdateMovie movie);
        Task<int> DeleteAsync(long id);
        Task<IEnumerable<Movie>> GetByGenreAsync(string genre);
        Task<List<Movie>> GetTopNAsync(int n);

    }
}
