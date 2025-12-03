using FluentValidation;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Users;

namespace LaTiendecicaEnLinea.Api.Identity.Validations.Users
{
    public class PasswordChangeRequestValidator : AbstractValidator<PasswordChangeRequest>
    {
        public PasswordChangeRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.")
                .MinimumLength(6).WithMessage("Current password must be at least 6 characters long.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .Matches(@"[!@#$%^&*(),.?""':{}|<>]").WithMessage("Current password must contain at least one special character.")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters long.");
        }
    }
}
