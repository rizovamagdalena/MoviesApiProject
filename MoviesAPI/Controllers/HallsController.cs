using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HallsController : ControllerBase
    {
        private readonly IHallService _hallService;

        public HallsController(IHallService hallService)
        {
            _hallService = hallService;
        }

        // GET api/halls
        [HttpGet]
        public async Task<ActionResult<List<HallDto>>> GetAll()
            => Ok(await _hallService.GetAllAsync());

        // GET api/halls/2
        [HttpGet("{id}")]
        public async Task<ActionResult<HallDto>> GetById(int id)
        {
            var hall = await _hallService.GetByIdAsync(id);
            return hall is null ? NotFound() : Ok(hall);
        }

        // GET api/halls/3/seats
        [HttpGet("{id}/seats")]
        public async Task<ActionResult<List<HallSeatDto>>> GetSeats(int id)
        {
            var seats = await _hallService.GetSeatsByHallIdAsync(id);
            return Ok(seats);
        }

        // GET api/halls/3/freeslots/2026-03-06
        [HttpGet("{hallId}/freeslots/{date}")]
        public async Task<ActionResult<List<string>>> GetAvailableTimeSlots(int hallId, DateOnly date)
        {
            var slots = await _hallService.GetAvailableTimeSlotsAsync(hallId, date);
            return Ok(slots);
        }
    }
}
