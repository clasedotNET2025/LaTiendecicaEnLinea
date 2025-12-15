namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Admin.Responses
{
    public class UserRolesResponse
    {
        public required string UserId { get; init; }
        public required string Email { get; init; }
        public required List<string> Roles { get; init; }
    }
}
