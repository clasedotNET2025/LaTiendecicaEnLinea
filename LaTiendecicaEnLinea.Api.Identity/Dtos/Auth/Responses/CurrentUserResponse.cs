namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Auth.Responses
{
    public class CurrentUserResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
    }
}
