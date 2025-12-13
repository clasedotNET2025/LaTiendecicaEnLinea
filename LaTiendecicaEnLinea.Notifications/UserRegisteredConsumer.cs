using LaTiendecicaEnLinea.Shared;
using MassTransit;

namespace LaTiendecicaEnLinea.Notifications;

internal class UserRegisteredConsumer : IConsumer<UserCreatedEvent>
{

    private ILogger<UserRegisteredConsumer> _logger;
    private IEmailService _emailService;

    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {

        var user = context.Message;

        _logger.LogInformation("New user registered: {UserId}, Email: {Email}", user.userId, user.email);

        await _emailService.SendWelcomeMail(user.email);

        _logger.LogInformation("Welcome email sent for user: {UserId}", user.userId);
    }
}