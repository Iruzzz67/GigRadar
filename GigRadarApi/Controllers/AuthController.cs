using Microsoft.AspNetCore.Mvc;
using GigRadarApi.Services;
using GigRadarApi.Models;

namespace GigRadarApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registrasi publik — role tetap "User"; bila client memilih Artist/EO,
        /// permohonan disimpan sebagai Pending dan diverifikasi admin (§25).
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var (success, message, user, token) = await _authService.RegisterAsync(
                request.Name, request.Email, request.Password, request.RequestedRole);

            if (!success)
                return BadRequest(new { message });

            return Ok(new
            {
                message,
                token,
                user = new { user!.UserId, user.Name, user.Email, user.Role, user.RoleStatus }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (success, message, user, token) = await _authService.LoginAsync(request.Email, request.Password);

            if (!success)
                return Unauthorized(new { message });

            return Ok(new
            {
                message,
                token,
                user = new { user!.UserId, user.Name, user.Email, user.Role, user.RoleStatus, user.City }
            });
        }
    }

    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        /// <summary>Permohonan role opsional: "User" (default), "Artist", atau "EO". "Admin" diabaikan server.</summary>
        public string? RequestedRole { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
