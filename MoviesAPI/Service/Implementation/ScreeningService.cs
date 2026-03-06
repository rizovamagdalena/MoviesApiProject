using MoviesAPI.Models;
using MoviesAPI.Repositories;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;

namespace MoviesAPI.Service.Implementation
{
    public class ScreeningService : IScreeningService
    {
        private readonly IScreeningRepository _screeningRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDbConnectionFactory _connectionFactory;

        public ScreeningService(
            IScreeningRepository screeningRepository,
            ITicketRepository ticketRepository,
            IUserRepository userRepository,
            IDbConnectionFactory connectionFactory)
        {
            _screeningRepository = screeningRepository;
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<ScreeningResponse>> GetAllAsync()
            => await _screeningRepository.GetAllAsync();

        public async Task<ScreeningResponse?> GetByIdAsync(long id)
            => await _screeningRepository.GetByIdAsync(id);

        public async Task<int> CreateAsync(CreateScreening screening)
            => await _screeningRepository.CreateAsync(screening);

        public async Task<int> UpdateAsync(long id, UpdateScreening screening)
        {
            if (screening.Available_Tickets > screening.Total_Tickets)
                throw new ArgumentException("Available tickets cannot exceed total tickets.");

            return await _screeningRepository.UpdateAsync(id, screening);
        }

        public async Task<int> DeleteAsync(long id)
            => await _screeningRepository.DeleteAsync(id);

        public async Task<List<SeatForScreeningDto>> GetReservedSeatsAsync(long screeningId)
            => await _screeningRepository.GetReservedSeatsAsync(screeningId);

        public async Task BookSeatsAsync(long screeningId, string username, List<int> hallSeatIds)
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                //  Resolve user
                var userId = await _userRepository.GetUserIdByUsernameAsync(username);
                if (userId == null)
                    throw new Exception($"User '{username}' not found.");

                //  Check for seat conflicts
                var takenSeats = await _screeningRepository.GetTakenSeatIdsAsync(
                    screeningId, hallSeatIds, conn, transaction);

                if (takenSeats.Any())
                    throw new Exception($"Seats already booked: {string.Join(", ", takenSeats)}");

                //  Get screening details for ticket price
                var screening = await _screeningRepository.GetByIdAsync(screeningId);
                if (screening == null)
                    throw new Exception("Screening not found.");

                //  Create a ticket + seat record per selected seat
                foreach (var seatId in hallSeatIds)
                {
                    var ticketId = await _ticketRepository.CreateAsync(
                        screening.Movie_Id, userId.Value,
                        screening.Screening_Date_Time,
                        screening.Movie.Amount,
                        seatId, conn, transaction);

                    await _screeningRepository.InsertSeatForScreeningAsync(
                        screeningId, seatId, userId.Value, ticketId, conn, transaction);
                }

                //  Update available ticket count
                await _screeningRepository.DecrementAvailableTicketsAsync(
                    screeningId, hallSeatIds.Count, conn, transaction);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
