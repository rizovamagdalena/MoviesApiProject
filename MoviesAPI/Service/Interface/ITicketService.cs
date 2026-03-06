using MoviesAPI.Models;

namespace MoviesAPI.Service.Interface
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketResponse>> GetAllAsync();
        Task<TicketResponse?> GetByIdAsync(long id);
        Task<int> DeleteAsync(long id);
    }
}
