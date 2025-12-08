using Asp.Versioning;
using FluentValidation;
using LaTiendecicaEnLinea.Api.Identity.Data;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LaTiendecicaEnLinea.Api.Identity.Controllers
{
    [ApiVersion(1)]
    [ApiController]
    [Route("/api/v{version:apiVersion}/admin/roles/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RoleController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public RoleController(
            RoleManager<IdentityRole> roleManager,
            ILogger<RoleController> logger,
            UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet("get_roles")]
        [ProducesResponseType<IEnumerable<RoleResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetRoles()
        {
            _logger.LogInformation("Fetching all roles");

            var roles = _roleManager.Roles.ToList();

            var response = roles.Select(role => new RoleResponse
            {
                RoleId = role.Id,
                RoleName = role.Name!,
                NormalizedName = role.NormalizedName!
            }).ToList();

            return Ok(response);
        }

        [HttpGet("get_role/{roleId}")]
        [ProducesResponseType<RoleDetailResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RoleDetailResponse>> GetRoleById(string roleId)
        {
            _logger.LogInformation("Fetching role: {RoleId}", roleId);

            var role = await _roleManager.FindByIdAsync(roleId);

            if (role is null)
            {
                _logger.LogWarning("Role not found: {RoleId}", roleId);
                return NotFound(new ProblemDetails
                {
                    Title = "Role not found",
                    Detail = $"Role with ID '{roleId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var response = new RoleDetailResponse
            {
                RoleId = role.Id,
                RoleName = role.Name!,
                NormalizedName = role.NormalizedName!,
                ConcurrencyStamp = role.ConcurrencyStamp
            };

            return Ok(response);
        }

        [HttpPost("create")]
        [ProducesResponseType<RoleResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RoleResponse>> CreateRole(
            [FromBody] CreateRoleRequest request,
            [FromServices] IValidator<CreateRoleRequest> validator,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.ToDictionary();
                _logger.LogWarning("Role creation validation failed for: {RoleName}", request.RoleName);
                return ValidationProblem(new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed"
                });
            }

            _logger.LogInformation("Creating role: {RoleName}", request.RoleName);

            var existingRole = await _roleManager.FindByNameAsync(request.RoleName);
            if (existingRole != null)
            {
                _logger.LogWarning("Role already exists: {RoleName}", request.RoleName);
                return Problem(
                    title: "Role already exists",
                    detail: $"Role '{request.RoleName}' already exists",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var role = new IdentityRole(request.RoleName);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("Role creation failed for {RoleName}: {Errors}",
                    request.RoleName, string.Join(", ", errors));
                return Problem(
                    title: "Failed to create role",
                    detail: string.Join(", ", errors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            _logger.LogInformation("Role created successfully: {RoleName}", request.RoleName);

            var response = new RoleResponse
            {
                RoleId = role.Id,
                RoleName = role.Name!,
                NormalizedName = role.NormalizedName!
            };

            return CreatedAtAction(nameof(GetRoleById), new { roleId = role.Id }, response);
        }

        [HttpPut("update/{roleId}")]
        [ProducesResponseType<RoleResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RoleResponse>> UpdateRole(
            string roleId,
            [FromBody] UpdateRoleRequest request,
            [FromServices] IValidator<UpdateRoleRequest> validator,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.ToDictionary();
                _logger.LogWarning("Role update validation failed for role: {RoleId}", roleId);
                return ValidationProblem(new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed"
                });
            }

            _logger.LogInformation("Updating role: {RoleId} to {NewName}", roleId, request.RoleName);

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role is null)
            {
                _logger.LogWarning("Role not found for update: {RoleId}", roleId);
                return NotFound(new ProblemDetails
                {
                    Title = "Role not found",
                    Detail = $"Role with ID '{roleId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var existingRole = await _roleManager.FindByNameAsync(request.RoleName);
            if (existingRole != null && existingRole.Id != roleId)
            {
                _logger.LogWarning("Role name already exists: {RoleName}", request.RoleName);
                return Problem(
                    title: "Role name already exists",
                    detail: $"Role name '{request.RoleName}' is already in use",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            role.Name = request.RoleName;
            role.NormalizedName = request.RoleName.ToUpperInvariant();

            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("Role update failed for {RoleId}: {Errors}",
                    roleId, string.Join(", ", errors));
                return Problem(
                    title: "Failed to update role",
                    detail: string.Join(", ", errors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            _logger.LogInformation("Role updated successfully: {RoleId} -> {NewName}", roleId, request.RoleName);

            var response = new RoleResponse
            {
                RoleId = role.Id,
                RoleName = role.Name!,
                NormalizedName = role.NormalizedName!
            };

            return Ok(response);
        }

        [HttpDelete("delete/{roleId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            _logger.LogInformation("Attempting to delete role: {RoleId}", roleId);

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role is null)
            {
                _logger.LogWarning("Role not found for deletion: {RoleId}", roleId);
                return NotFound(new ProblemDetails
                {
                    Title = "Role not found",
                    Detail = $"Role with ID '{roleId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            if (IsSystemRole(role.Name!))
            {
                _logger.LogWarning("Attempted to delete system role: {RoleName}", role.Name);
                return Problem(
                    title: "Cannot delete system role",
                    detail: $"Role '{role.Name}' is a system role and cannot be deleted.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Any())
            {
                _logger.LogInformation("Removing {UserCount} users from role {RoleName} before deletion",
                    usersInRole.Count, role.Name);

                foreach (var user in usersInRole)
                {
                    var removeResult = await _userManager.RemoveFromRoleAsync(user, role.Name!);
                    if (!removeResult.Succeeded)
                    {
                        var errors = removeResult.Errors.Select(e => e.Description);
                        _logger.LogWarning("Failed to remove user {UserId} from role {RoleName}: {Errors}",
                            user.Id, role.Name, string.Join(", ", errors));
                    }
                }

                _logger.LogInformation("Successfully removed all users from role {RoleName}", role.Name);
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("Role deletion failed for {RoleId} ({RoleName}): {Errors}",
                    roleId, role.Name, string.Join(", ", errors));
                return Problem(
                    title: "Failed to delete role",
                    detail: string.Join(", ", errors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            _logger.LogInformation("Role deleted successfully: {RoleId} ({RoleName}). Removed {UserCount} users from role.",
                roleId, role.Name, usersInRole.Count);

            return NoContent();
        }

        [HttpGet("get/{roleId}/users")]
        [ProducesResponseType<IEnumerable<UserInRoleResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserInRoleResponse>>> GetRoleUsers(string roleId)
        {
            _logger.LogInformation("Fetching users for role: {RoleId}", roleId);

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role is null)
            {
                _logger.LogWarning("Role not found: {RoleId}", roleId);
                return NotFound(new ProblemDetails
                {
                    Title = "Role not found",
                    Detail = $"Role with ID '{roleId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var users = await _userManager.GetUsersInRoleAsync(role.Name!);

            _logger.LogInformation("Found {UserCount} users in role {RoleName}", users.Count, role.Name);

            var response = users.Select(user => new UserInRoleResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                LockoutEnd = user.LockoutEnd?.UtcDateTime
            }).ToList();

            return Ok(response);
        }

        private bool IsSystemRole(string roleName)
        {
            var systemRoles = new[] { Roles.Admin, Roles.Customer, "SuperAdmin", "System" };
            return systemRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }
    }
}