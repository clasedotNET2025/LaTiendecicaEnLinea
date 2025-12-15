using LaTiendecicaEnLinea.Api.Identity.Dtos.Users.Requests;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Users.Responses;

namespace LaTiendecicaEnLinea.Api.Identity.Services
{
    public interface IUserService
    {
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<UserDetailResponse?> GetUserProfileAsync(string userId);
        Task<UserResponse> UpdateCurrentUserProfileAsync(string userId, UserProfileRequest request);
    }
}