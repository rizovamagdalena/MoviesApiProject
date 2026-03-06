using MoviesAPI.Models;
using Npgsql;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MoviesAPI.Repositories.Interface
{
    public interface IScreeningRepository
    {
        Task<IEnumerable<ScreeningResponse>> GetAllAsync();
        Task<ScreeningResponse> GetByIdAsync(long id);
        Task<Screening> GetForUpdateAsync(long id);

        Task<int> CreateAsync(CreateScreening screening);
        Task<int> UpdateAsync(long id, UpdateScreening screening);
        Task<int> DeleteAsync(long id);
        Task <List<SeatForScreeningDto>> GetReservedSeatsAsync(long id);
        Task<List<int>> GetTakenSeatIdsAsync(long screeningId, List<int> hallSeatIds, NpgsqlConnection conn, NpgsqlTransaction transaction);
        Task InsertSeatForScreeningAsync(long screeningId, int hallSeatId, long userId, int ticketId, NpgsqlConnection conn, NpgsqlTransaction transaction);
        Task DecrementAvailableTicketsAsync(long screeningId, int count, NpgsqlConnection conn, NpgsqlTransaction transaction);
        Task<List<Screening>> GetByHallAndDateAsync(int hallId, DateOnly date);
        Task<List<Screening>> GetByDateAsync(DateOnly date);
    }
}
