namespace LaTiendecicaEnLinea.Api.Identity.Dtos.Auth
{
    public class ResponseLogin
    {
        public required string Token { get; set; }
        public DateTime ExpirationAtUtc { get; set; }
    }
}
