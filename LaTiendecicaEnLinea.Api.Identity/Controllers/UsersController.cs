using Asp.Versioning;
using FluentValidation;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Users;
using LaTiendecicaEnLinea.Api.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LaTiendecicaEnLinea.Api.Identity.Controllers
{
    [ApiVersion(1)]
    [ApiController]
    [Route("/api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("get-my-profile")]
        [ProducesResponseType<UserDetailResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDetailResponse>> GetCurrentUserProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized profile fetch attempt - no user ID found in claims");
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "User ID not found in claims",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            _logger.LogInformation("Fetching profile for user: {UserId}", userId);

            var result = await _userService.GetUserProfileAsync(userId);

            if (result == null)
            {
                _logger.LogWarning("Profile not found for user: {UserId}", userId);
                return NotFound(new ProblemDetails
                {
                    Title = "User profile not found",
                    Detail = $"Profile for user '{userId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            _logger.LogInformation("Profile retrieved successfully for user: {UserId}", userId);
            return Ok(result);
        }

        [HttpPut("update-my-profile")]
        [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserResponse>> UpdateMyProfile(
            [FromBody] UserProfileRequest request,
            [FromServices] IValidator<UserProfileRequest> validator)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized profile update attempt - no user ID found in claims");
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "User ID not found in claims",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.ToDictionary();
                _logger.LogWarning("Profile update validation failed for user: {UserId}", userId);
                return ValidationProblem(new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed"
                });
            }

            _logger.LogInformation("Updating profile for user: {UserId}", userId);

            var result = await _userService.UpdateCurrentUserProfileAsync(userId, request);

            if (!string.IsNullOrEmpty(result.Message) &&
                (result.Message.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("already taken", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Profile update failed for user {UserId}: {Message}", userId, result.Message);

                if (result.Message.Contains("already taken"))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "UserName already taken",
                        Detail = result.Message,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                if (result.Message.Contains("not found"))
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "User not found",
                        Detail = result.Message,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return BadRequest(new ProblemDetails
                {
                    Title = "Failed to update profile",
                    Detail = result.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            _logger.LogInformation("Profile updated successfully for user: {UserId}", userId);
            return Ok(result);
        }

        [HttpPost("me/password")]
        [ProducesResponseType<PasswordChangeResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PasswordChangeResponse>> ChangeMyPassword(
            [FromBody] PasswordChangeRequest request,
            [FromServices] IValidator<PasswordChangeRequest> validator)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized password change attempt - no user ID found in claims");
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "User ID not found in claims",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.ToDictionary();
                _logger.LogWarning("Password change validation failed for user: {UserId}", userId);
                return ValidationProblem(new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed"
                });
            }

            _logger.LogInformation("Changing password for user: {UserId}", userId);

            var result = await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

            var response = new PasswordChangeResponse
            {
                Success = result
            };

            if (!result)
            {
                _logger.LogWarning("Password change failed for user: {UserId}", userId);
                return BadRequest(new ProblemDetails
                {
                    Title = "Failed to change password",
                    Detail = "Unable to change password. Please check your current password and try again.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return Ok(response);
        }
    }
}