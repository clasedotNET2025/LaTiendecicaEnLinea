namespace LaTiendecicaEnLinea.Notifications
{
    internal interface IEmailService
    {
        Task SendWelcomeMail(string toEmail);
    }
}