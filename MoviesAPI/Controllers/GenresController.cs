using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Service.Interface;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly IGenreService _genreService;

        public GenresController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        // GET: api/genres
        [HttpGet]
        public async Task<ActionResult<List<string>>> GetAll()
        {
            var genres = await _genreService.GetAllAsync();
            if (!genres.Any()) return NotFound("No genres found.");
            return Ok(genres);
        }
    }
}
