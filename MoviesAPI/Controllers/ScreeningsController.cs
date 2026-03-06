using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models;
using MoviesAPI.Models.System;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScreeningsController : ControllerBase
    {
        private readonly IScreeningService _screeningService;

        public ScreeningsController(IScreeningService screeningService)
        {
            _screeningService = screeningService;
        }

        // GET api/screenings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScreeningResponse>>> GetAll()
             => Ok(await _screeningService.GetAllAsync());

        // GET api/screenings/2
        [HttpGet("{id}")]
        public async Task<ActionResult<ScreeningResponse>> GetById(long id)
        {
            var screening = await _screeningService.GetByIdAsync(id);
            return screening is null ? NotFound() : Ok(screening);
        }

        // POST api/screenings
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateScreening screening)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _screeningService.CreateAsync(screening);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        // PUT api/screenings/2
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(long id, [FromBody] UpdateScreening screening)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _screeningService.GetByIdAsync(id);
            if (existing is null) return NotFound();

            try
            {
                var result = await _screeningService.UpdateAsync(id, screening);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/screenings/2
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _screeningService.GetByIdAsync(id);
            if (existing is null) return NotFound();

            await _screeningService.DeleteAsync(id);
            return NoContent();
        }

        // GET api/screenings/2/reservedseats
        [HttpGet("{id}/reservedseats")]
        public async Task<ActionResult<List<SeatForScreeningDto>>> GetReservedSeats(long id)
        {
            var seats = await _screeningService.GetReservedSeatsAsync(id);
            return Ok(seats);
        }

        // POST api/screenings/2/book
        [HttpPost("{id}/book")]
        public async Task<IActionResult> BookSeats(long id, [FromBody] BookSeatsRequest request)
        {
            if (request.SelectedSeatsId == null || !request.SelectedSeatsId.Any())
                return BadRequest("No seats selected.");

            try
            {
                await _screeningService.BookSeatsAsync(id, request.username, request.SelectedSeatsId);
                return Ok(new { Message = "Seats successfully booked!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
