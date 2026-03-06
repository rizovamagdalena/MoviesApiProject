using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models.System;
using MoviesAPI.Repositories.Interface;
using MoviesAPI.Service.Interface;
using System.Reflection;
using System.Text;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public AccountController(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request");

            var user = await _userService.LoginAsync(request.Username, request.Password);

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

            var success = await _userService.LogoutAsync(username);

            if (!success)
                return NotFound("User not found or already logged out");

            return Ok(new { message = "Logout successful", username });

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid request");

            if (await _userService.IsEmailTakenAsync(request.Email))
                return Conflict(new { message = "Email already registered." });

            var existingUser = await _userService.GetByUsernameAsync(request.Username);
            if (existingUser != null)
                return Conflict(new { message = "Username already exists." });


            var token = Guid.NewGuid().ToString();

            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/Account/ConfirmEmail?token={token}";

            TempRegistrationStore.Add(token, request);

            await _emailService.SendEmailAsync(new EmailMessage
            {
                MailTo = request.Email,
                Subject = "Confirm your email",
                Content = BuildConfirmationEmail(request.Name, confirmationLink)
            });

            return Ok(new { message = "Confirmation email sent. Please check your email to complete registration." });
        }



        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var pendingRequest = TempRegistrationStore.Get(token);
            if (pendingRequest == null)
                return BadRequest("Invalid or expired token");

            pendingRequest.isActive = true;
            await _userService.CreateAsync(pendingRequest);

            return Ok("Email confirmed and user account created! You can now log in.");
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required.");

            var user = await _userService.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // Always return success message to avoid exposing valid emails
                return Ok(new { message = "If this email exists, a reset link has been sent." });
            }

            // Generate a reset token
            var token = Guid.NewGuid().ToString();

            // Store token temporarily (or use a proper DB table in production)
            TempResetPasswordStore.Add(token, request.Email);

            // Build reset link
            var resetLink = $"https://localhost:7268/Account/ResetPassword?token={token}";


            // Build email content
            await _emailService.SendEmailAsync(new EmailMessage
            {
                MailTo = request.Email,
                Subject = "Reset Your Password",
                Content = BuildResetPasswordEmail(user.Name, resetLink)
            });

            return Ok(new { message = "If this email exists, a reset link has been sent." });
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var email = TempResetPasswordStore.GetEmail(request.Token);
            if (email == null)
                return BadRequest("Invalid or expired token.");

            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
                return BadRequest("User not found.");

            // TODO: Hash password before saving in production
            await _userService.UpdatePasswordAsync(user.Id, request.NewPassword);

            return Ok(new { message = "Password successfully reset. You can now log in." });
        }

        private static string BuildConfirmationEmail(string name, string confirmationLink)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><body>");
            sb.AppendLine($"<p>Hi <strong>{name}</strong>,</p>");
            sb.AppendLine("<p>Thank you for registering at <strong>Movie App</strong>!</p>");
            sb.AppendLine("<p>Please confirm your email by clicking the link below:</p>");
            sb.AppendLine($"<p><a href='{confirmationLink}' style='background-color:#4CAF50;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>Confirm your email</a></p>");
            sb.AppendLine("<p>If you did not register, please ignore this email.</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string BuildResetPasswordEmail(string name, string resetLink)
        {
            return $@"
                <html><body>
                <p>Hi {name},</p>
                <p>Click the link below to reset your password:</p>
                <p><a href='{resetLink}' style='background-color:#4CAF50;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>Reset Password</a></p>
                <p>If you did not request this, please ignore this email.</p>
                </body></html>";
        }
        public static class TempRegistrationStore
        {
            private static readonly Dictionary<string, RegisterRequest> _pending = new();

            public static void Add(string token, RegisterRequest request)
                => _pending[token] = request;

            public static RegisterRequest? Get(string token)
            {
                if (!_pending.TryGetValue(token, out var request)) return null;
                _pending.Remove(token);
                return request;
            }
        }

        public static class TempResetPasswordStore
        {
            private static readonly Dictionary<string, string> _pending = new();

            public static void Add(string token, string email)
                => _pending[token] = email;

            public static string? GetEmail(string token)
            {
                if (!_pending.TryGetValue(token, out var email)) return null;
                _pending.Remove(token);
                return email;
            }
        }

        public class ResetPasswordRequest
        {
            public string Token { get; set; }
            public string NewPassword { get; set; }
        }
    }


}
