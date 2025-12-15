using FluentValidation;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Auth.Requests;

namespace LaTiendecicaEnLinea.Api.Identity.Validations.Users
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
