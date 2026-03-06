using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;

namespace MoviesAPI.Service.Implementation
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<IEnumerable<TicketResponse>> GetAllAsync()
            => await _ticketRepository.GetAllAsync();

        public async Task<TicketResponse?> GetByIdAsync(long id)
            => await _ticketRepository.GetByIdAsync(id);

        public async Task<int> DeleteAsync(long id)
            => await _ticketRepository.DeleteAsync(id);
    }
}
