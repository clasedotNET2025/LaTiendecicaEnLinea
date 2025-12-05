namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Auth
{
    public class RegisterResponse
    {
        public required string UserId { get; init; }
        public required string Email { get; init; }
        public required string Message { get; init; }
    }
}
