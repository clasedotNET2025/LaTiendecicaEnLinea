using Asp.Versioning;
using LaTiendecicaEnLinea.Api.Identity.Services;
using Microsoft.AspNetCore.Mvc;

namespace LaTiendecicaEnLinea.Api.Identity.Controllers
{
    [ApiVersion(1)]
    [ApiController]
    [Route("/api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private IEnumerable<User> _users = new List<User>();
        private IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;

        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var result = await _authService.Register(user.Email, user.Password);

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var result = await _authService.Login(user.Email, user.Password);

            if (result == null)
            {
                return Unauthorized();
            }

            return Ok(result);
        }

    }

    public class User
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
