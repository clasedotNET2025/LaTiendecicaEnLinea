namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Admin
{
    public class AdminUserDetailResponse
    {
        public required string UserId { get; init; }
        public required string Email { get; init; }
        public required string UserName { get; init; }
        public bool EmailConfirmed { get; init; }
        public required List<string> Roles { get; init; }
        public string CreatedAt { get; init; } = string.Empty;
        public bool IsLocked { get; init; }
        public DateTime? LockoutEnd { get; init; }
    }
}
