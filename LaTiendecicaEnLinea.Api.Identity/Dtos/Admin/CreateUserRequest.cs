namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Admin
{
    public class CreateUserRequest
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
        public bool EmailConfirmed { get; init; } = true; // Por defecto confirmado
        public List<string>? Roles { get; init; } // Roles opcionales
    }
}
