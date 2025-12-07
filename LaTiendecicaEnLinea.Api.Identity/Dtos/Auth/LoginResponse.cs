namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Auth
{
    public class LoginResponse
    {
        public required string AccessToken { get; init; }
        public required string TokenType { get; init; }
        public required int ExpiresIn { get; init; }
        public required string UserId { get; init; }
        public required string Email { get; init; }
        public required IList<string> Roles { get; init; }
    }
}
