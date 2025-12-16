using Asp.Versioning;
using FluentValidation;
using LaTiendecicaEnLinea.Api.Identity.Data;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Auth.Requests;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Auth.Responses;
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
    /// <summary>
    /// Controller for managing user authentication, authorization, and identity operations.
    /// Provides endpoints for user registration, login, token generation, and user information retrieval.
    /// Implements RESTful API design principles with proper HTTP methods and status codes.
    /// </summary>
    /// <remarks>
    /// This controller handles the complete authentication flow including:
    /// <list type="bullet">
    /// <item><description>User registration with role assignment</description></item>
    /// <item><description>User login with JWT token generation</description></item>
    /// <item><description>Current user information retrieval</description></item>
    /// <item><description>Role-based access control demonstration</description></item>
    /// </list>
    /// </remarks>
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

        /// <summary>
        /// Constructs a new AuthController with the required dependencies.
        /// </summary>
        /// <param name="userManager">ASP.NET Core Identity UserManager for user operations.</param>
        /// <param name="signInManager">ASP.NET Core Identity SignInManager for authentication operations.</param>
        /// <param name="configuration">Application configuration settings provider.</param>
        /// <param name="logger">Logger instance for recording controller events.</param>
        /// <param name="publishEndpoint">MassTransit endpoint for publishing integration events.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">Registration data containing user credentials.</param>
        /// <param name="validator">FluentValidation validator for request validation.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>Created response with user information.</returns>
        /// <response code="201">User successfully registered. Returns user details.</response>
        /// <response code="400">Bad request due to validation errors or existing user.</response>
        /// <response code="500">Internal server error during user creation.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RegisterResponse>> Register(
            [FromBody] RegisterRequest request,
            [FromServices] IValidator<RegisterRequest> validator,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Starting registration process for email: {Email}", request.Email);

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.ToDictionary();
                _logger.LogWarning("Registration validation failed for email: {Email}", request.Email);
                return ValidationProblem(new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed",
                    Detail = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                return Problem(
                    title: "User already exists",
                    detail: $"A user with email '{request.Email}' already exists.",
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

                _logger.LogDebug("UserCreatedEvent published for user: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the registration
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

        /// <summary>
        /// Authenticates a user and returns a JWT access token.
        /// </summary>
        /// <param name="request">Login credentials containing email and password.</param>
        /// <param name="validator">FluentValidation validator for request validation.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>JWT token and user information upon successful authentication.</returns>
        /// <response code="200">Authentication successful. Returns access token and user data.</response>
        /// <response code="400">Bad request due to validation errors or account lockout.</response>
        /// <response code="401">Unauthorized due to invalid credentials.</response>
        /// <response code="500">Internal server error during authentication.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromBody] LoginRequest request,
            [FromServices] IValidator<LoginRequest> validator,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Starting login process for email: {Email}", request.Email);

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.ToDictionary();
                _logger.LogWarning("Login validation failed for email: {Email}", request.Email);
                return ValidationProblem(new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed",
                    Detail = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest
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

        /// <summary>
        /// Retrieves information about the currently authenticated user.
        /// </summary>
        /// <returns>Detailed information about the authenticated user.</returns>
        /// <response code="200">Returns current user information.</response>
        /// <response code="401">User is not authenticated or token is invalid/expired.</response>
        /// <response code="404">User not found in database (rare case).</response>
        /// <response code="500">Internal server error during user retrieval.</response>
        [HttpGet("me")]
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetCurrentUser called without valid NameIdentifier claim");
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found in database for ID: {UserId}", userId);
                return Problem(
                    title: "User not found",
                    detail: $"User with ID '{userId}' was not found in the database.",
                    statusCode: StatusCodes.Status404NotFound);
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

            _logger.LogDebug("User information retrieved: {UserId} - {Email}", userId, user.Email);

            return Ok(response);
        }

        /// <summary>
        /// Demonstrates an endpoint accessible only to users with the Admin role.
        /// </summary>
        /// <returns>Admin-specific information and confirmation of admin status.</returns>
        /// <response code="200">User is an admin, returns admin information.</response>
        /// <response code="401">User is not authenticated or token is invalid.</response>
        /// <response code="403">User is authenticated but does not have the Admin role.</response>
        [HttpGet("admin-only")]
        [Authorize(Roles = Roles.Admin)]
        [Produces("application/json")]
        [ProducesResponseType(typeof(AdminOnlyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Generates a JWT token for an authenticated user.
        /// </summary>
        /// <param name="user">The IdentityUser for whom to generate the token.</param>
        /// <param name="roles">Collection of roles assigned to the user.</param>
        /// <returns>A signed JWT token as a string.</returns>
        /// <exception cref="InvalidOperationException">Thrown when JWT configuration is missing or invalid.</exception>
        private string GenerateJwtToken(IdentityUser user, IList<string> roles)
        {
            var jwtSecret = _configuration["Jwt:Secret"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");

            if (string.IsNullOrEmpty(jwtSecret))
                throw new InvalidOperationException("JWT Secret is not configured.");
            if (string.IsNullOrEmpty(jwtIssuer))
                throw new InvalidOperationException("JWT Issuer is not configured.");
            if (string.IsNullOrEmpty(jwtAudience))
                throw new InvalidOperationException("JWT Audience is not configured.");

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