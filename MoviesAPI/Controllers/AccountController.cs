using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models.System;
using MoviesAPI.Repositories.Interface;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request");

            var user = await _userRepository.GetUserByUsernameAndPassword(request.Username, request.Password);

            if (user == null)
                return Unauthorized("Invalid username or password");




            // Add laterr: For now, just returning a simple token (can be replaced with JWT)
            var token = Guid.NewGuid().ToString();

            var response = new LoginResponse
            {
                Token = token,
                User = user,
                Error = null
            };

          
            //System.Diagnostics.Debug.WriteLine("first name:" + response.User.Name);
            //System.Diagnostics.Debug.WriteLine("last name:" + response.User.IsActive);


            return Ok(response);
        }


        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username is required");

            var success = await _userRepository.LogoutUser(username);

            if (!success)
                return NotFound("User not found or already logged out");

            return Ok(new { message = "Logout successful", username });

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request");

            var existingUser = await _userRepository.GetUserByUsername(request.Username);

            if (existingUser != null)
                return Unauthorized("Already Exists USername");

            var userId = await _userRepository.CreateUserAsync(request);
            var response = true;

            request.isActive = false;

            return Ok(response);
        }
    }
}
