using MoviesAPI.Models;

namespace MoviesAPI.Service.Interface
{
    public interface IScreeningService
    {
        Task<IEnumerable<ScreeningResponse>> GetAllAsync();
        Task<ScreeningResponse?> GetByIdAsync(long id);
        Task<int> CreateAsync(CreateScreening screening);
        Task<int> UpdateAsync(long id, UpdateScreening screening);
        Task<int> DeleteAsync(long id);
        Task<List<SeatForScreeningDto>> GetReservedSeatsAsync(long screeningId);
        Task BookSeatsAsync(long screeningId, string username, List<int> hallSeatIds);
    }
}
