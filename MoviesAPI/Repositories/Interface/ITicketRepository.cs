using MoviesAPI.Models;

namespace MoviesAPI.Repositories.Interface
{
    public interface ITicketRepository
    {
        Task<IEnumerable<TicketResponse>> GetTicketsAsync();
        Task<TicketResponse> GetTicketAsync(long id);
        Task<int> CreateTicketAsync(CreateTicket ticket);
        //Task<int> UpdateTicketAsync(long id, UpdateTicket ticket);
        //Task<UpdateTicket> GetTicketForUpdateAsync(long id);
        Task<int> DeleteTicketAsync(long id);
    }
}
