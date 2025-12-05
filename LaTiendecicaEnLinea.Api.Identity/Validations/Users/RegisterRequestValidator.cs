using FluentValidation;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Auth;

namespace LaTiendecicaEnLinea.Api.Identity.Validations.Users
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"\d").WithMessage("Password must contain at least one number");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).When(x => !string.IsNullOrEmpty(x.ConfirmPassword))
                .WithMessage("Passwords do not match");
        }
    }
}
