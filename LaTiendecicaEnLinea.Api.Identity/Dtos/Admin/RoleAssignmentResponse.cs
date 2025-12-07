namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Admin
{
    public class RoleAssignmentResponse
    {
        public required string UserId { get; init; }
        public required string Email { get; init; }
        public required string RoleName { get; init; }
        public required string Message { get; init; }
    }
}
