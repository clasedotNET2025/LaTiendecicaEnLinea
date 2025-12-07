namespace LaTiendecicaEnLinea.Api.Identity.Validations.Roles
{
    public class UserInRoleResponse
    {
        public required string UserId { get; init; }
        public required string Email { get; init; }
        public required string UserName { get; init; }
        public bool EmailConfirmed { get; init; }
        public string? PhoneNumber { get; init; }
        public DateTime? LockoutEnd { get; init; }
    }
}
