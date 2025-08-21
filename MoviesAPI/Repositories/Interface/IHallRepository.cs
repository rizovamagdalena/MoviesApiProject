using MoviesAPI.Models;

namespace MoviesAPI.Repositories.Interface
{
    public interface IHallRepository
    {
        Task<List<HallDto>> GetAllHallsAsync();
        Task<HallDto?> GetHallByIdAsync(int hallId);
        Task<List<HallSeatDto>> GetSeatsByHallIdAsync(int hallId);
    }


}
