namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Roles.Responses
{
    public class RoleDetailResponse
    {
        public required string RoleId { get; init; }
        public required string RoleName { get; init; }
        public required string NormalizedName { get; init; }
        public string? ConcurrencyStamp { get; init; }
    }
}
