namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Admin.Requests
{
    public class LockUserRequest
    {
        public int? LockoutMinutes { get; init; } // null = lock permanente
    }
}
