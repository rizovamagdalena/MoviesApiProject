using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;

namespace MoviesAPI.Service.Implementation
{
    public class HallService : IHallService
    {
        private readonly IHallRepository _hallRepository;
        private readonly IScreeningRepository _screeningRepository;

        public HallService(IHallRepository hallRepository)
        {
            _hallRepository = hallRepository;
        }

        public async Task<List<HallDto>> GetAllAsync()
            => await _hallRepository.GetAllAsync();

        public async Task<HallDto?> GetByIdAsync(int hallId)
            => await _hallRepository.GetByIdAsync(hallId);

        public async Task<List<HallSeatDto>> GetSeatsByHallIdAsync(int hallId)
            => await _hallRepository.GetSeatsByHallIdAsync(hallId);

        public async Task<List<string>> GetAvailableTimeSlotsAsync(int hallId, DateOnly date)
        {
            var allSlots = new List<TimeOnly>
        {
            new TimeOnly(11, 0, 0),
            new TimeOnly(14, 0, 0),
            new TimeOnly(17, 0, 0),
            new TimeOnly(20, 0, 0),
            new TimeOnly(23, 0, 0)
        };

            var bookedScreenings = await _screeningRepository.GetByHallAndDateAsync(hallId, date);

            var bookedTimes = bookedScreenings
                .Select(s => new TimeOnly(s.Screening_Date_Time.Hour, s.Screening_Date_Time.Minute, 0))
                .ToList();

            return allSlots
                .Where(t => !bookedTimes.Contains(t))
                .Select(t => t.ToString("HH:mm"))
                .ToList();
        }
    }
}
