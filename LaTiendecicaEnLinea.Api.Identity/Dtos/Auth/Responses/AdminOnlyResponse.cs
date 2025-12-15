namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Auth.Responses
{
    public class AdminOnlyResponse
    {
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsAdmin { get; set; }
    }
}
