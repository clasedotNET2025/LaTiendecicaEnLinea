using FluentValidation;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Users;

namespace LaTiendecicaEnLinea.Api.Identity.Validations.Users
{
    public class UpdateProfileRequestValidator : AbstractValidator<UserProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().When(x => x.UserName != null)
                .WithMessage("UserName cannot be empty")
                .MinimumLength(3).When(x => x.UserName != null)
                .WithMessage("UserName must be at least 3 characters")
                .MaximumLength(50)
                .WithMessage("UserName cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9._-]+$").When(x => x.UserName != null)
                .WithMessage("UserName can only contain letters, numbers, dots, dashes and underscores");
        }
    }
}
