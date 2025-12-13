using Asp.Versioning;
using FluentValidation;
using LaTiendecicaEnLinea.Api.Identity.Data;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Auth;
using LaTiendecicaEnLinea.Shared;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LaTiendecicaEnLinea.Api.Identity.Controllers
{
    [ApiVersion(1)]
    [ApiController]
    [Route("/api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterResponse>> Register(
            [FromBody] RegisterRequest request,
            [FromServices] IValidator<RegisterRequest> validator,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.ToDictionary();
                _logger.LogWarning("Registration validation failed for email: {Email}", request.Email);
                return ValidationProblem(new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed"
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                return Problem(
                    title: "User already exists",
                    detail: "User with this email already exists",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("User creation failed for email: {Email}. Errors: {@Errors}",
                    request.Email, errors);
                return Problem(
                    title: "Failed to create user",
                    detail: string.Join(", ", errors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            await _userManager.AddToRoleAsync(user, Roles.Customer);

            _logger.LogInformation("User successfully registered: {UserId} - {Email}", user.Id, user.Email);

            try
            {
                await _publishEndpoint.Publish(new UserCreatedEvent(
                    userId: user.Id,
                    email: user.Email
                ), cancellationToken);

                _logger.LogInformation("UserCreatedEvent published for user: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                // Log del error pero no fallar el registro
                _logger.LogError(ex, "Failed to publish UserCreatedEvent for user: {UserId}. User was still created.", user.Id);
            }

            var response = new RegisterResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Message = "User registered successfully"
            };

            return CreatedAtAction(nameof(Login), new { }, response);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromBody] LoginRequest request,
            [FromServices] IValidator<LoginRequest> validator,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.ToDictionary();
                _logger.LogWarning("Login validation failed for email: {Email}", request.Email);
                return ValidationProblem(new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed"
                });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                return Problem(
                    title: "Invalid credentials",
                    detail: "Invalid email or password",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out: {Email}", request.Email);
                return Problem(
                    title: "Account locked",
                    detail: "Account is locked due to multiple failed login attempts. Please try again later.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
                return Problem(
                    title: "Invalid credentials",
                    detail: "Invalid email or password",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = GenerateJwtToken(user, roles);
            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");

            _logger.LogInformation("User successfully logged in: {UserId} - {Email}", user.Id, user.Email);

            var response = new LoginResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = expiryMinutes * 60,
                UserId = user.Id,
                Email = user.Email!,
                Roles = roles
            };

            return Ok(response);
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var response = new CurrentUserResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                Roles = roles,
                UserName = user.UserName!,
                EmailConfirmed = user.EmailConfirmed
            };

            _logger.LogInformation("User information retrieved: {UserId} - {Email}", userId, user.Email);

            return Ok(response);
        }

        [HttpGet("admin-only")]
        [ProducesResponseType<AdminOnlyResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<AdminOnlyResponse> AdminOnly()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            var response = new AdminOnlyResponse
            {
                Message = "You are an admin!",
                UserId = userId ?? string.Empty,
                UserEmail = userEmail ?? string.Empty,
                Timestamp = DateTime.UtcNow,
                IsAdmin = true
            };

            _logger.LogInformation("Admin endpoint accessed by user: {UserId} - {UserEmail}", userId, userEmail);

            return Ok(response);
        }

        private string GenerateJwtToken(IdentityUser user, IList<string> roles)
        {
            var jwtSecret = _configuration["Jwt:Secret"]!;
            var jwtIssuer = _configuration["Jwt:Issuer"]!;
            var jwtAudience = _configuration["Jwt:Audience"]!;
            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}