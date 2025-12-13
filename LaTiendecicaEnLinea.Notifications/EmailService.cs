using MailKit.Net.Smtp;
using System;
using System.Collections.Generic;
using System.Text;

namespace LaTiendecicaEnLinea.Notifications
{
    internal class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(
            ILogger<EmailService> logger,
            IConfiguration configuration
        )
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendWelcomeMail(string toEmail)
        {
            try
            {
                using var client = new SmtpClient();

                var host = _configuration.GetSection("Email:SmtpHost").Value;
                var port = int.Parse(_configuration.GetSection("Email:SmtpPort").Value!);
                
                await client.ConnectAsync(host, port, false);

                var fromEmail = _configuration.GetSection("Email:FromAddress").Value;
                var message = new MimeKit.MimeMessage();

                message.From.Add(new MimeKit.MailboxAddress("LaTiendecicaEnLinea", fromEmail!));
                message.To.Add(new MimeKit.MailboxAddress("Test", toEmail));
                message.Subject = "Welcome to LaTiendecicaEnLinea!";
                message.Body = new MimeKit.TextPart("plain")
                {
                    Text = "Thank you for registering with LaTiendecicaEnLinea. We're excited to have you on board!"
                };
                
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                _logger.LogInformation("Welcome email sent to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {ToEmail}", toEmail);
                throw;
            }
        }
    }
}
