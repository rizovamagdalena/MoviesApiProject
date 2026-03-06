using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;

namespace MoviesAPI.Service.Implementation
{
    public class FutureMovieService : IFutureMovieService
    {
        private readonly IFutureMovieRepository _futureMovieRepository;

        public FutureMovieService(IFutureMovieRepository futureMovieRepository)
        {
            _futureMovieRepository = futureMovieRepository;
        }

        public async Task<IEnumerable<FutureMovie>> GetAllAsync()
        {
            return await _futureMovieRepository.GetAllAsync();
        }

        public async Task<FutureMovie?> GetByIdAsync(long id)
        {
            return await _futureMovieRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(CreateFutureMovie movie)
        {
            return await _futureMovieRepository.CreateAsync(movie);
        }

        public async Task<int> DeleteAsync(long id)
        {
            return await _futureMovieRepository.DeleteAsync(id);
        }
    }

}
