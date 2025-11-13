using MoviesAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MoviesAPI.Repositories.Interface
{
    public interface IScreeningRepository
    {
        Task<IEnumerable<ScreeningResponse>> GetScreeningsAsync();
        Task<ScreeningResponse> GetScreeningAsync(long id);
        Task<int> CreateScreeningAsync(CreateScreening screening);
        Task<int> UpdateScreeningAsync(long id, UpdateScreening screening);
        Task<Screening> GetScreeningForUpdateAsync(long id);
        Task<int> DeleteScreeningAsync(long id);
        Task <List<SeatForScreeningDto>> GetReservedSeatsAsync(long id);
        Task BookSeatsAsync(long screeningId, string username, List<int> hallSeatIds);
        Task<List<Screening>> GetScreeningsByHallAndDateAsync(int hallId,DateOnly date);
        Task<List<Screening>> GetScreeningsByDateAsync(DateOnly date);
    }
}
