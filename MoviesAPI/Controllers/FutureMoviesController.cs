using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models;
using MoviesAPI.Service.Interface;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FutureMoviesController : ControllerBase
    {
        private readonly IFutureMovieService _futureMovieService;

        public FutureMoviesController(IFutureMovieService futureMovieService)
        {
            _futureMovieService = futureMovieService;
        }

        // GET: api/futuremovies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FutureMovie>>> GetAll()
        {
            var movies = await _futureMovieService.GetAllAsync();
            if (!movies.Any()) return NotFound("No upcoming movies found.");
            return Ok(movies);
        }

        // GET: api/futuremovies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FutureMovie>> GetById(long id)
        {
            var movie = await _futureMovieService.GetByIdAsync(id);
            if (movie == null) return NotFound();
            return Ok(movie);
        }

        // POST: api/futuremovies
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateFutureMovie movie)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var id = await _futureMovieService.CreateAsync(movie);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        // DELETE: api/futuremovies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _futureMovieService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _futureMovieService.DeleteAsync(id);
            return NoContent();
        }
    }
}