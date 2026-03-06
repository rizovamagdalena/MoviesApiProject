using MoviesAPI.Models;
using Npgsql;
using System.Threading.Tasks;

namespace MoviesAPI.Repositories.Interface
{
    public interface ITicketRepository
    {
        Task<IEnumerable<TicketResponse>> GetAllAsync();
        Task<TicketResponse?> GetByIdAsync(long id);
        Task<int> CreateAsync(long movieId, long userId, DateTime watchMovie, decimal price, int hallSeatId, NpgsqlConnection conn, NpgsqlTransaction transaction);
        Task<int> DeleteAsync(long id);
    }
}
