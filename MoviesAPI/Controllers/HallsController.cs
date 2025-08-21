using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Repositories.Interface;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HallsController : ControllerBase
    {
        private readonly IHallRepository _hallRepository;
        private readonly IScreeningRepository _screeningRepository;

        public HallsController(IHallRepository hallRepository, IScreeningRepository screeningRepository)
        {
            _hallRepository = hallRepository;
            _screeningRepository = screeningRepository;
        }

        // GET: api/halls
        [HttpGet]
        public async Task<IActionResult> GetAllHalls()
        {
            var halls = await _hallRepository.GetAllHallsAsync();
            return Ok(halls);
        }

        // GET api/halls/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHallById(int id)
        {
            var hall = await _hallRepository.GetHallByIdAsync(id);
            if (hall == null) return NotFound();
            return Ok(hall);
        }

        // GET: api/halls/1/seats
        [HttpGet("{id}/seats")]
        public async Task<IActionResult> GetSeatsByHallId(int id)
        {
            var seats = await _hallRepository.GetSeatsByHallIdAsync(id);
            return Ok(seats);
        }


        // GET: api/halls/1/freeslots/01-01-2001
        [HttpGet("{hallId}/freeslots/{date}")]
        public async Task<IActionResult> GetAvailableTimeSlots(int hallId, DateOnly date)
        {
            var allSlots = new List<TimeOnly> {
                new TimeOnly(11,0,0),
                new TimeOnly(14,0,0),
                new TimeOnly(17,0,0),
                new TimeOnly(20,0,0),
                new TimeOnly(23,0,0)
            };

            var bookedScreenings = await _screeningRepository.GetScreeningsByHallAndDateAsync(hallId, date);


            var bookedTimes = bookedScreenings
                  .Select(s => new TimeOnly(s.Screening_Date_Time.Hour, s.Screening_Date_Time.Minute, 0))
                  .ToList();

            var allowedSlots = allSlots
                    .Where(t => !bookedTimes.Contains(t))
                    .Select(t => t.ToString("HH:mm"))
                    .ToList();
            //System.Diagnostics.Debug.WriteLine($"allowedSlots: {allowedSlots}");


            return Ok(allowedSlots);
        }
    }
}
