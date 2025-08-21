using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models;
using MoviesAPI.Models.System;
using MoviesAPI.Repositories.Interface;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScreeningsController : ControllerBase
    {
        private readonly IScreeningRepository _screeningRepository;

        public ScreeningsController(IScreeningRepository screeningRepository)
        {
            _screeningRepository = screeningRepository;
        }


        // GET: api/<ScreeningsController> or GET: api/screenings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScreeningResponse>>> Get()
        {
            var screenings = await _screeningRepository.GetScreeningsAsync();
            return Ok(screenings);
        }

        // GET api/<ScreeningsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ScreeningResponse>> Get(long id)
        {
            var screening = await _screeningRepository.GetScreeningAsync(id);

            if (screening == null)
            {
                return NotFound();
            }

            return Ok(screening);
        }

        // POST api/<ScreeningsController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateScreening screening)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);



            var id = await _screeningRepository.CreateScreeningAsync(screening);
            return Ok(id);
        }

        // PUT api/<ScreeningsController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] UpdateScreening screening)
        {
            var existing = await _screeningRepository.GetScreeningForUpdateAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (screening.Available_Tickets > screening.Total_Tickets)
            {
                return BadRequest("Available tickets cannot be greater than total tickets.");
            }

            var result = await _screeningRepository.UpdateScreeningAsync(id, screening);
            return Ok(result);
        }

        // DELETE api/<ScreeningsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _screeningRepository.GetScreeningAsync(id);
            if (existing == null)
                return NotFound();

            var result = await _screeningRepository.DeleteScreeningAsync(id);
            return Ok(result);
        }

        // GET: api/screenings/1/reservedseats
        [HttpGet("{id}/reservedseats")]
        public async Task<IActionResult> GetReservedSeatsForScreening(int id)
        {
            var reservedSeats = await _screeningRepository.GetReservedSeatsAsync(id);

            if (reservedSeats == null || !reservedSeats.Any())
                reservedSeats = new List<SeatForScreeningDto>();

            return Ok(reservedSeats);
        }

        // POST: api/screenings/5/book
        [HttpPost("{id}/book")]
        public async Task<IActionResult> BookSeats(int id, [FromBody] BookSeatsRequest request)
        {
            if (request.SelectedSeatsId == null || !request.SelectedSeatsId.Any())
                return BadRequest("No seats selected.");

            var screening = await _screeningRepository.GetScreeningAsync(id);

            if (screening == null)
                return NotFound("Screening not found.");

            try
            {
                await _screeningRepository.BookSeatsAsync(id, request.username, request.SelectedSeatsId);
                return Ok(new { Message = "Seats successfully booked!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
