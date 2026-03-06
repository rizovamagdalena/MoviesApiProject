using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;

namespace MoviesAPI.Service.Implementation
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genreRepository;

        public GenreService(IGenreRepository genreRepository)
        {
            _genreRepository = genreRepository;
        }

        public async Task<List<string>> GetAllAsync()
        {
            return await _genreRepository.GetAllAsync();
        }
    }
}
