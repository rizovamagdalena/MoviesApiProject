using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models;
using MoviesAPI.Repositories.Interface;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {

        private readonly IMovieRepository _movieRepository;

        public MoviesController(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }


        // GET: api/<MoviesController> or GET: api/movies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movie>>> Get()
        {
            var movies = await _movieRepository.GetMoviesAsync();
            return Ok(movies);
        }

        // GET api/<MoviesController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Movie>> Get(long id)
        {
            var movie = await _movieRepository.GetMovieAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            return Ok(movie);
        }

        // POST api/<MoviesController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateAndUpdateMovie movie)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _movieRepository.CreateMovieAsync(movie);
            return Ok(id);
        }

        // PUT api/<MoviesController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] CreateAndUpdateMovie movie)
        {
            var existing = await _movieRepository.GetMovieForUpdateAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _movieRepository.UpdateMovieAsync(id, movie);
            return Ok(result);
        }

        // DELETE api/<MoviesController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _movieRepository.GetMovieAsync(id);
            if (existing == null)
                return NotFound();

            var result = await _movieRepository.DeleteMovieAsync(id);
            return Ok(result);
        }

        // GET: /api/movies/genre/{genreName}
        [HttpGet("genre/{genreName}")]
        public async Task<ActionResult<List<Movie>>> GetMoviesByGenre(string genreName)
        {
            var movies = await _movieRepository.GetMoviesByGenreAsync(genreName);
            if (!movies.Any()) return NotFound($"No movies found for genre '{genreName}'.");

            return Ok(movies);
        }

        // GET: /api/movies/genres}
        [HttpGet("genres")]
        public async Task<ActionResult<List<string>>> GetAllGenres()
        {
            var genres = await _movieRepository.GettAllGenresAsync();

            return Ok(genres);
        }

        // GET: /api/movies/futuremovies
        [HttpGet("futuremovies")]
        public async Task<ActionResult<List<FutureMovie>>> GetFutureMovies()
        {
            var movies = await _movieRepository.GetFutureMoviesAsync();
            if (!movies.Any()) return NotFound($"No movies found .");

            return Ok(movies);
        }

        // POST api/movies/futuremovies
        [HttpPost("futuremovies")]
        public async Task<ActionResult> Post([FromBody] CreateFutureMovie movie)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _movieRepository.CreateFutureMovieAsync(movie);
            return Ok(id);
        }

        // DELETE api/movies/futuremovies/5
        [HttpDelete("futuremovies/{id}")]
        public async Task<IActionResult> DeleteFutureMovie(long id)
        {
            var existing = await _movieRepository.GetFutureMovieAsync(id);
            if (existing == null)
                return NotFound();

            var result = await _movieRepository.DeleteFutureMovieAsync(id);
            return Ok(result);
        }

        // GET: /api/movies/5/rating/3
        [HttpGet("{idMovie}/rating/{idUser}")]
        public async Task<ActionResult<int>> GetRatingOfUserForMovie(long idMovie, long idUser)
        {
            var rating = await _movieRepository.GetRatingOfUserForMovieAsync(idMovie, idUser);
            if (rating == null)
                return NotFound();

            return Ok(rating);
        }

        // GET: /api/movies/toprated/4
        [HttpGet("toprated/{n}")]
        public async Task<ActionResult<List<Movie>>> GetTopNRated(int n)
        {
            var movies = await _movieRepository.GetTopNMoviesAsync(n);

            if (movies == null || movies.Count == 0)
                return NotFound();

            return Ok(movies);
        }


        // PUT: /api/movies/5/rating
        [HttpPut("{movieId}/rating")]
        public async Task<ActionResult> UpdateRating(long movieId, [FromBody] CreateRating rating)
        {
            if (rating.Rating < 1 || rating.Rating > 10)
                return BadRequest("Rating must be between 1 and 10.");

            // ensure movieId from route matches body
            rating.MovieId = movieId;

            var success = await _movieRepository.UpsertRating(rating);
            if (!success)
                return StatusCode(500, "Failed to save rating.");

            return Ok(new
            {
                success = true,
                message = "Rating saved successfully.",
                rating
            });
        }


    }
}
