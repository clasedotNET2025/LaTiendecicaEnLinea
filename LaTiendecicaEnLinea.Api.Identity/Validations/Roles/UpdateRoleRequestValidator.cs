using FluentValidation;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Roles.Requests;

namespace LaTiendecicaEnLinea.Api.Identity.Validations.Roles
{
    public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
    {
        public UpdateRoleRequestValidator()
        {
            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("Role name is required")
                .MinimumLength(3).WithMessage("Role name must be at least 3 characters")
                .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_]+$").WithMessage("Role name can only contain letters, numbers, spaces, hyphens and underscores")
                .Must(name => !name.StartsWith(" ")).WithMessage("Role name cannot start with a space")
                .Must(name => !name.EndsWith(" ")).WithMessage("Role name cannot end with a space");
        }
    }
}
