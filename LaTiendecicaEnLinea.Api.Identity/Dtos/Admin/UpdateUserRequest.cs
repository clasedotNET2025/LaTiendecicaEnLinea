namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Admin
{
    public class UpdateUserRequest
    {
        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }
        public bool? EmailConfirmed { get; init; }
    }
}
