using Asp.Versioning;
using LaTiendecicaEnLinea.Api.Identity.Data;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Admin.Requests;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Admin.Responses;
using LaTiendecicaEnLinea.Api.Identity.Dtos.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LaTiendecicaEnLinea.Api.Identity.Controllers
{
    /// <summary>
    /// Controller for managing users and roles by administrators
    /// </summary>
    [ApiVersion(1)]
    [ApiController]
    [Route("/api/v{version:apiVersion}/admin/users")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class AdminUserController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminUserController> _logger;
        private readonly IDistributedCache _cache;

        /// <summary>
        /// Initializes a new instance of the AdminUserController class
        /// </summary>
        public AdminUserController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminUserController> logger,
            IDistributedCache cache = null)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Retrieves a paginated list of all users in the system
        /// </summary>
        [HttpGet]
        [ProducesResponseType<PaginatedResponse<AdminUserResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PaginatedResponse<AdminUserResponse>>> GetUsers(
            [FromQuery] PaginationParams paginationParams)
        {
            paginationParams.Normalize();

            _logger.LogInformation("Admin fetching users - Page: {Page}, PageSize: {PageSize}",
                paginationParams.Page, paginationParams.PageSize);

            var query = _userManager.Users;
            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(paginationParams.SortBy))
            {
                query = paginationParams.SortDirection == "desc"
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email);
            }

            var users = await query
                .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            var responseList = new List<AdminUserResponse>();

            foreach (var user in users)
            {
                var roles = await GetUserRolesCached(user.Id);
                responseList.Add(new AdminUserResponse
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles,
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                    LockoutEnd = user.LockoutEnd?.UtcDateTime
                });
            }

            var response = new PaginatedResponse<AdminUserResponse>(
                responseList,
                paginationParams.Page,
                paginationParams.PageSize,
                totalCount);

            if (Request != null)
            {
                response.FirstPageUrl = GeneratePageUrl(1);
                response.PreviousPageUrl = response.HasPreviousPage ? GeneratePageUrl(paginationParams.Page - 1) : null;
                response.NextPageUrl = response.HasNextPage ? GeneratePageUrl(paginationParams.Page + 1) : null;
                response.LastPageUrl = GeneratePageUrl(response.TotalPages);
            }

            return Ok(response);
        }

        /// <summary>
        /// Retrieves detailed information about a specific user by their ID
        /// </summary>
        [HttpGet("{userId}")]
        [ProducesResponseType<AdminUserDetailResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AdminUserDetailResponse>> GetUserById(string userId)
        {
            _logger.LogInformation("Fetching user details: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return NotFound(new ProblemDetails
                {
                    Title = "User not found",
                    Detail = $"User with ID '{userId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var response = new AdminUserDetailResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList(),
                CreatedAt = user.Id,
                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = user.LockoutEnd?.UtcDateTime
            };

            _logger.LogInformation("User details retrieved: {UserId} - {Email}", userId, user.Email);

            return Ok(response);
        }

        /// <summary>
        /// Creates a new user account with specified roles and email confirmation status
        /// </summary>
        [HttpPost]
        [ProducesResponseType<AdminUserResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AdminUserResponse>> CreateUser(
            [FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CreateUser: {@Errors}",
                    ModelState.Values.SelectMany(v => v.Errors));
                return ValidationProblem(ModelState);
            }

            _logger.LogInformation("Admin creating new user: {Email}", request.Email);

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Create user attempt with existing email: {Email}", request.Email);
                return Problem(
                    title: "User already exists",
                    detail: "User with this email already exists",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = request.EmailConfirmed
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("User creation failed for email: {Email}. Errors: {@Errors}",
                    request.Email, errors);
                return Problem(
                    title: "Failed to create user",
                    detail: string.Join(", ", errors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (request.Roles != null && request.Roles.Any())
            {
                foreach (var role in request.Roles)
                {
                    var roleExists = await _roleManager.RoleExistsAsync(role);
                    if (roleExists)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                    else
                    {
                        _logger.LogWarning("Role '{Role}' does not exist, skipping assignment for user {Email}",
                            role, request.Email);
                    }
                }
            }
            else
            {
                await _userManager.AddToRoleAsync(user, Roles.Customer);
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            if (_cache != null)
            {
                var cacheKey = $"user_roles_{user.Id}";
                await _cache.RemoveAsync(cacheKey);
            }

            _logger.LogInformation("User created successfully by admin: {UserId} - {Email}", user.Id, user.Email);

            var response = new AdminUserResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                EmailConfirmed = user.EmailConfirmed,
                Roles = userRoles.ToList()
            };

            return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, response);
        }

        /// <summary>
        /// Updates an existing user's information such as email and confirmation status
        /// </summary>
        [HttpPut("{userId}")]
        [ProducesResponseType<AdminUserResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AdminUserResponse>> UpdateUser(
            string userId,
            [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for UpdateUser: {@Errors}",
                    ModelState.Values.SelectMany(v => v.Errors));
                return ValidationProblem(ModelState);
            }

            _logger.LogInformation("Admin updating user: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("User not found for update: {UserId}", userId);
                return NotFound(new ProblemDetails
                {
                    Title = "User not found",
                    Detail = $"User with ID '{userId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Email already exists: {Email}", request.Email);
                    return Problem(
                        title: "Email already in use",
                        detail: $"Email '{request.Email}' is already registered",
                        statusCode: StatusCodes.Status400BadRequest);
                }

                user.Email = request.Email;
                user.UserName = request.Email;
            }

            if (request.EmailConfirmed.HasValue)
            {
                user.EmailConfirmed = request.EmailConfirmed.Value;
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description);
                _logger.LogWarning("User update failed for {UserId}: {Errors}",
                    userId, string.Join(", ", errors));
                return Problem(
                    title: "Failed to update user",
                    detail: string.Join(", ", errors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (_cache != null)
            {
                var cacheKey = $"user_roles_{user.Id}";
                await _cache.RemoveAsync(cacheKey);
            }

            _logger.LogInformation("User updated successfully: {UserId}", userId);

            var userRoles = await _userManager.GetRolesAsync(user);

            var response = new AdminUserResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                EmailConfirmed = user.EmailConfirmed,
                Roles = userRoles.ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Deletes a user account from the system
        /// </summary>
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            _logger.LogInformation("Admin deleting user: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("User not found for deletion: {UserId}", userId);
                return NotFound(new ProblemDetails
                {
                    Title = "User not found",
                    Detail = $"User with ID '{userId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
            if (isAdmin)
            {
                _logger.LogWarning("Attempted to delete admin user: {UserId} ({Email})", userId, user.Email);
                return Problem(
                    title: "Cannot delete admin user",
                    detail: "Admin users cannot be deleted. Remove admin role first or use a different method.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("User deletion failed for {UserId} ({Email}): {Errors}",
                    userId, user.Email, string.Join(", ", errors));
                return Problem(
                    title: "Failed to delete user",
                    detail: string.Join(", ", errors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (_cache != null)
            {
                var cacheKey = $"user_roles_{user.Id}";
                await _cache.RemoveAsync(cacheKey);
            }

            _logger.LogInformation("User deleted successfully: {UserId} ({Email}). Had roles: {Roles}",
                userId, user.Email, string.Join(", ", userRoles));

            return NoContent();
        }

        /// <summary>
        /// Locks a user account to prevent login
        /// </summary>
        [HttpPost("{userId}/lock")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> LockUser(
            string userId,
            [FromBody] LockUserRequest? request = null)
        {
            _logger.LogInformation("Admin locking user: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("User not found for lock: {UserId}", userId);
                return NotFound(new ProblemDetails
                {
                    Title = "User not found",
                    Detail = $"User with ID '{userId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            DateTimeOffset? lockoutEnd = null;

            if (request?.LockoutMinutes.HasValue == true && request.LockoutMinutes.Value > 0)
            {
                lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(request.LockoutMinutes.Value);
                _logger.LogInformation("Locking user {UserId} for {Minutes} minutes until {LockoutEnd}",
                    userId, request.LockoutMinutes.Value, lockoutEnd);
            }
            else
            {
                lockoutEnd = DateTimeOffset.MaxValue;
                _logger.LogInformation("Locking user {UserId} permanently", userId);
            }

            user.LockoutEnd = lockoutEnd;
            user.LockoutEnabled = true;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("User lock failed for {UserId}: {Errors}",
                    userId, string.Join(", ", errors));
                return Problem(
                    title: "Failed to lock user",
                    detail: string.Join(", ", errors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            _logger.LogInformation("User locked successfully: {UserId} until {LockoutEnd}",
                userId, lockoutEnd);

            return NoContent();
        }

        /// <summary>
        /// Unlocks a previously locked user account
        /// </summary>
        [HttpPost("{userId}/unlock")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            _logger.LogInformation("Admin unlocking user: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("User not found for unlock: {UserId}", userId);
                return NotFound(new ProblemDetails
                {
                    Title = "User not found",
                    Detail = $"User with ID '{userId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            if (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow)
            {
                _logger.LogInformation("User {UserId} is not currently locked", userId);
            }

            user.LockoutEnd = null;
            user.AccessFailedCount = 0;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("User unlock failed for {UserId}: {Errors}",
                    userId, string.Join(", ", errors));
                return Problem(
                    title: "Failed to unlock user",
                    detail: string.Join(", ", errors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            _logger.LogInformation("User unlocked successfully: {UserId}", userId);

            return NoContent();
        }

        /// <summary>
        /// Retrieves all roles assigned to a specific user
        /// </summary>
        [HttpGet("{userId}/roles")]
        [ProducesResponseType<UserRolesResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserRolesResponse>> GetUserRoles(string userId)
        {
            _logger.LogInformation("Fetching roles for user: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return NotFound(new ProblemDetails
                {
                    Title = "User not found",
                    Detail = $"User with ID '{userId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var response = new UserRolesResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                Roles = roles.ToList()
            };

            _logger.LogInformation("Found {RoleCount} roles for user {UserId}: {Roles}",
                roles.Count, userId, string.Join(", ", roles));

            return Ok(response);
        }

        /// <summary>
        /// Assigns a specific role to a user
        /// </summary>
        [HttpPost("{userId}/roles/{roleName}")]
        [ProducesResponseType<RoleAssignmentResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RoleAssignmentResponse>> AssignRoleToUser(
            string userId,
            string roleName)
        {
            _logger.LogInformation("Assigning role {RoleName} to user: {UserId}", roleName, userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return NotFound(new ProblemDetails
                {
                    Title = "User not found",
                    Detail = $"User with ID '{userId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                _logger.LogWarning("Role does not exist: {RoleName}", roleName);
                return NotFound(new ProblemDetails
                {
                    Title = "Role not found",
                    Detail = $"Role '{roleName}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (isInRole)
            {
                _logger.LogWarning("User {UserId} already has role {RoleName}", userId, roleName);
                return Problem(
                    title: "Role already assigned",
                    detail: $"User already has role '{roleName}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("Role assignment failed for {UserId}: {Errors}",
                    userId, string.Join(", ", errors));
                return Problem(
                    title: "Failed to assign role",
                    detail: string.Join(", ", errors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (_cache != null)
            {
                var cacheKey = $"user_roles_{user.Id}";
                await _cache.RemoveAsync(cacheKey);
            }

            _logger.LogInformation("Role {RoleName} assigned to user {UserId} ({Email}) successfully",
                roleName, userId, user.Email);

            var response = new RoleAssignmentResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                RoleName = roleName,
                Message = $"Role '{roleName}' assigned successfully to user {user.Email}"
            };

            return Ok(response);
        }

        /// <summary>
        /// Removes a specific role from a user
        /// </summary>
        [HttpDelete("{userId}/roles/{roleName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveRoleFromUser(
            string userId,
            string roleName)
        {
            _logger.LogInformation("Removing role {RoleName} from user: {UserId}", roleName, userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return NotFound(new ProblemDetails
                {
                    Title = "User not found",
                    Detail = $"User with ID '{userId}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                _logger.LogWarning("Role does not exist: {RoleName}", roleName);
                return NotFound(new ProblemDetails
                {
                    Title = "Role not found",
                    Detail = $"Role '{roleName}' does not exist",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (!isInRole)
            {
                _logger.LogWarning("User {UserId} does not have role {RoleName}", userId, roleName);
                return Problem(
                    title: "Role not assigned",
                    detail: $"User does not have role '{roleName}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (roleName.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
                if (isAdmin)
                {
                    var allAdmins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
                    if (allAdmins.Count <= 1)
                    {
                        _logger.LogWarning("Cannot remove Admin role from last admin user: {UserId}", userId);
                        return Problem(
                            title: "Cannot remove Admin role",
                            detail: "Cannot remove Admin role from the last admin user in the system",
                            statusCode: StatusCodes.Status400BadRequest);
                    }
                }
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("Role removal failed for {UserId}: {Errors}",
                    userId, string.Join(", ", errors));
                return Problem(
                    title: "Failed to remove role",
                    detail: string.Join(", ", errors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (_cache != null)
            {
                var cacheKey = $"user_roles_{user.Id}";
                await _cache.RemoveAsync(cacheKey);
            }

            _logger.LogInformation("Role {RoleName} removed from user {UserId} ({Email}) successfully",
                roleName, userId, user.Email);

            return NoContent();
        }

        private async Task<List<string>> GetUserRolesCached(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            if (_cache == null)
            {
                return (await _userManager.GetRolesAsync(user)).ToList();
            }

            var cacheKey = $"user_roles_{userId}";
            var cachedRoles = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedRoles))
            {
                return JsonSerializer.Deserialize<List<string>>(cachedRoles) ?? new List<string>();
            }

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            await _cache.SetStringAsync(cacheKey,
                JsonSerializer.Serialize(roles),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            return roles;
        }

        private string? GeneratePageUrl(int page)
        {
            if (Request == null) return null;

            var query = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            query["page"] = page.ToString();

            var queryString = string.Join("&", query.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            return $"{Request.Path}?{queryString}";
        }
    }
}