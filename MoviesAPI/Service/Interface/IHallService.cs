using MoviesAPI.Models;

namespace MoviesAPI.Service.Interface
{
    public interface IHallService
    {
        Task<List<HallDto>> GetAllAsync();
        Task<HallDto?> GetByIdAsync(int hallId);
        Task<List<HallSeatDto>> GetSeatsByHallIdAsync(int hallId);
        Task<List<string>> GetAvailableTimeSlotsAsync(int hallId, DateOnly date); 

    }
}
