using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models;
using MoviesAPI.Repositories.Implementation;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {

        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }


        // GET: api/<MoviesController> or GET: api/movies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movie>>> GetAll()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            return Ok(movies);
        }

        // GET api/<MoviesController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Movie>> GetById(long id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            return Ok(movie);
        }

        // GET: /api/movies/genre/{genreName}
        [HttpGet("genre/{genreName}")]
        public async Task<ActionResult<List<Movie>>> GetByGenre(string genreName)
        {
            var movies = await _movieService.GetByGenreAsync(genreName);
            if (!movies.Any()) return NotFound($"No movies found for genre '{genreName}'.");

            return Ok(movies);
        }

        // GET: /api/movies/toprated/4
        [HttpGet("toprated/{n}")]
        public async Task<ActionResult<List<Movie>>> GetTopNRated(int n)
        {
            if (n <= 0) return BadRequest("n must be greater than 0.");

            var movies = await _movieService.GetTopRatedAsync(n);

            if (!movies.Any()) return NotFound();

            return Ok(movies);
        }


        // POST api/<MoviesController>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateAndUpdateMovie movie)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _movieService.CreateMovieAsync(movie);
            return Ok(id);
        }

        // PUT api/<MoviesController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] CreateAndUpdateMovie movie)
        {
            var existing = await _movieService.GetMovieByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _movieService.UpdateMovieAsync(id, movie);
            return Ok(result);
        }

        // DELETE api/<MoviesController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _movieService.GetMovieByIdAsync(id);
            if (existing == null)
                return NotFound();
            
            var result = await _movieService.DeleteMovieAsync(id);
            return Ok(result);
        }

   

    }
}
