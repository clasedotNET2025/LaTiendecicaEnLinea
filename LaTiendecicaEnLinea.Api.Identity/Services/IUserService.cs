namespace LaTiendecicaEnLinea.Api.Identity.Services
{
    public interface IUserService
    {
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}