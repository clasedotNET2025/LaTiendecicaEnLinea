using LaTiendecicaEnLinea.Api.Identity.Dtos.Auth;

namespace LaTiendecicaEnLinea.Api.Identity.Services
{
    public interface IAuthService
    {
        Task<bool> Register(string email, string password);
        Task<ResponseLogin?> Login(string email, string password);
    }
}