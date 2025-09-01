using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models.System;
using MoviesAPI.Repositories.Implementation;
using MoviesAPI.Repositories.Interface;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        // GET: api/<UsersController> or GET: api/movies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            var users = await _userRepository.GetUsersAsync();
            return Ok(users);
        }

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(long id)
        {
            var user = await _userRepository.GetUserAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // POST api/<UsersController>
        [HttpPost]
        public async Task<ActionResult> Post([FromForm] RegisterRequest user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _userRepository.CreateUserAsync(user);
            return Ok(id);
        }

        // PUT api/<UsersController>/5
        [HttpPut]
        public async Task<ActionResult> Put([FromForm] UserProfile user)
        {
            var existing = await _userRepository.GetUserByUsername(user.Username);
            if (existing == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _userRepository.UpdateUserAsync(user);
            return Ok(result);
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _userRepository.GetUserAsync(id);
            if (existing == null)
                return NotFound();

            var result = await _userRepository.DeleteUserAsync(id);
            return Ok(result);
        }
    }
}
