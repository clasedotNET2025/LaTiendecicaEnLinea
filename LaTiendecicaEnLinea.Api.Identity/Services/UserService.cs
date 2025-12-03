using Microsoft.AspNetCore.Identity;

namespace LaTiendecicaEnLinea.Api.Identity.Services
{
    public class UserService : IUserService
    {
        private UserManager<IdentityUser> _userManager;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<IdentityUser> userManager,
            ILogger<UserService> logger
            )
        {
            _userManager = userManager;
            _logger = logger;
        }

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
    }
}
