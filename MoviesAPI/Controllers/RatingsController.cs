using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models;
using MoviesAPI.Service.Interface;

namespace MoviesAPI.Controllers
{
    [Route("api/movies")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        // GET: api/movies/5/rating/3
        [HttpGet("{movieId}/rating/{userId}")]
        public async Task<ActionResult<MovieRating>> GetRatingOfUserForMovie(long movieId, long userId)
        {
            var rating = await _ratingService.GetRatingOfUserForMovieAsync(movieId, userId);
            if (rating == null) return NotFound();
            return Ok(rating);
        }

        // PUT: api/movies/5/rating
        [HttpPut("{movieId}/rating")]
        public async Task<ActionResult> UpsertRating(long movieId, [FromBody] CreateRating rating)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            rating.MovieId = movieId;

            try
            {
                var success = await _ratingService.UpsertRatingAsync(rating);
                if (!success) return StatusCode(500, "Failed to save rating.");

                return Ok(new { success = true, message = "Rating saved successfully.", rating });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
