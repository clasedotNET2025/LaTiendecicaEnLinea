using Microsoft.AspNetCore.Identity;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Users.Requests;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Users.Responses;

namespace LaTiendecicaEnLinea.Api.Identity.Services
{
    public class UserService : IUserService
    {
        // NOTA: Cambié a readonly para mejor práctica
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<IdentityUser> userManager,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // Método existente - mantenlo igual
        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error changing password for user {UserId}: {Error}", userId, error.Description);
                }
                return false;
            }

            _logger.LogInformation("Password changed successfully for user {UserId}.", userId);
            return true;
        }

        public async Task<UserDetailResponse?> GetUserProfileAsync(string userId)
        {
            _logger.LogInformation("Fetching profile for user: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            var response = new UserDetailResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList(),
                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = user.LockoutEnd?.UtcDateTime
            };

            _logger.LogInformation("Profile retrieved for user: {UserId}", userId);
            return response;
        }
        public async Task<UserResponse> UpdateCurrentUserProfileAsync(string userId, UserProfileRequest request)
        {
            _logger.LogInformation("Updating profile for user: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found for update: {UserId}", userId);
                return new UserResponse
                {
                    UserId = userId,
                    Email = "Not found",
                    UserName = "Not found",
                    EmailConfirmed = false,
                    Roles = new List<string>(),
                    IsLocked = false,
                    Message = "User not found"
                };
            }

            // Actualizar campos si se proporcionan
            var changesMade = false;

            if (!string.IsNullOrEmpty(request.UserName) && request.UserName != user.UserName)
            {
                // Verificar si el nuevo UserName ya existe
                var existingUser = await _userManager.FindByNameAsync(request.UserName);
                if (existingUser != null && existingUser.Id != userId)
                {
                    _logger.LogWarning("UserName already exists: {UserName}", request.UserName);
                    return new UserResponse
                    {
                        UserId = user.Id,
                        Email = user.Email!,
                        UserName = user.UserName!,
                        EmailConfirmed = user.EmailConfirmed,
                        Roles = await _userManager.GetRolesAsync(user) as List<string> ?? new List<string>(),
                        IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                        Message = $"UserName '{request.UserName}' is already taken"
                    };
                }

                user.UserName = request.UserName;
                changesMade = true;
                _logger.LogInformation("Updating UserName for user {UserId}", userId);
            }

            // Agregar más campos aquí según lo que quieras permitir actualizar
            // Ejemplo:
            // if (!string.IsNullOrEmpty(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
            // {
            //     user.PhoneNumber = request.PhoneNumber;
            //     changesMade = true;
            // }

            if (!changesMade)
            {
                _logger.LogInformation("No changes detected for user: {UserId}", userId);
                var roles = await _userManager.GetRolesAsync(user);

                return new UserResponse
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList(),
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                    LockoutEnd = user.LockoutEnd?.UtcDateTime,
                    Message = "No changes were made"
                };
            }

            // Guardar cambios
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to update user {UserId}: {Errors}", userId, errors);

                var roles = await _userManager.GetRolesAsync(user);

                return new UserResponse
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList(),
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                    LockoutEnd = user.LockoutEnd?.UtcDateTime,
                    Message = $"Update failed: {errors}"
                };
            }

            _logger.LogInformation("Profile updated successfully for user: {UserId}", userId);

            var updatedRoles = await _userManager.GetRolesAsync(user);

            return new UserResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                EmailConfirmed = user.EmailConfirmed,
                Roles = updatedRoles.ToList(),
                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = user.LockoutEnd?.UtcDateTime,
                Message = "Profile updated successfully"
            };
        }
    }
}