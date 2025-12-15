namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Auth.Responses
{
    public class ResponseLogin
    {
        public required string Token { get; set; }
        public DateTime ExpirationAtUtc { get; set; }
    }
}
