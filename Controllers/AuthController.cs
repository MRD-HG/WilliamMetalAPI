using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WilliamMetalAPI.Models;
using WilliamMetalAPI.Services;

namespace WilliamMetalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(new { success = true, data = response });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }

        // POST /api/Auth/register (temporary: open registration for testing)
        [HttpPost("register")]
        [AllowAnonymous] // remove this or change to [Authorize(Roles = "ADMIN")] in production
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _authService.RegisterAsync(request);
                return Created(string.Empty, new { success = true, data = user, message = "User created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while creating user" });
            }
        }

        [HttpGet("me")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCurrentUser()
        {
            // In this project we run without JWT middleware (single-user mode).
            // If no authenticated user is present, fallback to the first active user.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var user = await _authService.GetCurrentUserAsync(userId);
            if (user == null)
            {
                // Fallback: return admin/first user when no JWT claims are available.
                user = await _authService.GetDefaultUserAsync();
                if (user == null)
                    return NotFound(new { success = false, message = "User not found" });
            }

            return Ok(new { success = true, data = user });
        }

        [HttpPost("change-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userId))
            {
                var u = await _authService.GetDefaultUserAsync();
                userId = u?.Id ?? string.Empty;
            }

            var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            if (!result)
            {
                return BadRequest(new { success = false, message = "Failed to change password. Please check your current password." });
            }

            return Ok(new { success = true, message = "Password changed successfully" });
        }

        [HttpPost("validate-token")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateToken([FromBody] TokenValidationRequest request)
        {
            var isValid = await _authService.ValidateTokenAsync(request.Token);
            return Ok(new { success = true, valid = isValid });
        }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class TokenValidationRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}