namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Admin
{
    public class LockUserRequest
    {
        public int? LockoutMinutes { get; init; } // null = lock permanente
    }
}
