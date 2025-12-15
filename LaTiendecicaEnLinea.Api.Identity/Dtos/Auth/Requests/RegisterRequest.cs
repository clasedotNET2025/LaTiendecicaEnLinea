namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Auth.Requests
{
    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? ConfirmPassword { get; set; } // Opcional para validación
    }
}
