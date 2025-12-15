namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Users.Responses
{
    public class UserResponse
    {
        public required string UserId { get; init; }
        public required string Email { get; init; }
        public required string UserName { get; init; }
        public bool EmailConfirmed { get; init; }
        public required List<string> Roles { get; init; }
        public bool IsLocked { get; init; }
        public DateTime? LockoutEnd { get; init; }
        public string? Message { get; init; }
    }
}
