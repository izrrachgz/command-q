using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IdentityServer.Modules.Accounts.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IdentityServer.Modules.Accounts
{
    [ApiController, Route(AccountConstants.Routes.InternalAdminRoot)]
    public sealed class IdentityAdminController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IIdentitySecurityEventRecorder securityEvents,
        IOptions<IdentityAdminApiOptions> options) : ControllerBase
    {
        [HttpGet(AccountConstants.Routes.InternalAdminUsers)]
        public async Task<IActionResult> Users(
            [FromQuery] IdentityAdminSearchCriteria criteria,
            CancellationToken cancellationToken)
        {
            if (!HasAdminApiKey())
            {
                return Unauthorized();
            }

            var page = Math.Max(1, criteria.Page);
            var pageSize = Math.Clamp(criteria.Size, 1, AccountConstants.MaxAdminPageSize);
            var query = userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                var searchTerm = $"%{criteria.SearchTerm.Trim()}%";
                query = query.Where(user =>
                    user.UserName != null && EF.Functions.Like(user.UserName, searchTerm) ||
                    user.Email != null && EF.Functions.Like(user.Email, searchTerm));
            }

            if (criteria.StartDate.HasValue)
            {
                var startDateUtc = criteria.StartDate.Value.UtcDateTime;
                query = query.Where(user => user.CreatedUtc >= startDateUtc);
            }

            if (criteria.EndDate.HasValue)
            {
                var endDateUtc = criteria.EndDate.Value.UtcDateTime;
                query = query.Where(user => user.CreatedUtc <= endDateUtc);
            }

            var total = await query.LongCountAsync(cancellationToken);
            var users = await ApplySorting(query, criteria.SortBy, criteria.AscendingSort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var response = new List<IdentityAdminUserResponse>(users.Count);

            foreach (var user in users)
            {
                response.Add(await MapUser(user));
            }

            return Ok(CreatePage(response, total, page, pageSize));
        }

        [HttpGet(AccountConstants.Routes.InternalAdminUser)]
        public async Task<IActionResult> UserDetails(Guid userId)
        {
            if (!HasAdminApiKey())
            {
                return Unauthorized();
            }

            var user = await userManager.FindByIdAsync(userId.ToString());

            return user is null ? NotFound() : Ok(await MapUser(user));
        }

        [HttpGet(AccountConstants.Routes.InternalAdminRoles)]
        public async Task<IActionResult> Roles(CancellationToken cancellationToken)
        {
            if (!HasAdminApiKey())
            {
                return Unauthorized();
            }

            var roles = await roleManager.Roles
                .AsNoTracking()
                .OrderBy(role => role.Name)
                .Select(role => new IdentityAdminRoleResponse(role.Id, role.Name ?? string.Empty))
                .ToListAsync(cancellationToken);

            return Ok(roles);
        }

        [HttpPut(AccountConstants.Routes.InternalAdminUserRoles)]
        public async Task<IActionResult> UpdateUserRoles(
            Guid userId,
            UpdateIdentityUserRolesRequest request,
            CancellationToken cancellationToken)
        {
            if (!HasAdminApiKey())
            {
                return Unauthorized();
            }

            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return NotFound();
            }

            var roleNames = await roleManager.Roles
                .Select(role => role.Name ?? string.Empty)
                .Where(role => role != string.Empty)
                .ToListAsync(cancellationToken);
            var existingRoles = new HashSet<string>(roleNames, StringComparer.OrdinalIgnoreCase);
            var requestedRoles = request.Roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!requestedRoles.Contains(AccountConstants.DefaultRole, StringComparer.OrdinalIgnoreCase))
            {
                requestedRoles.Add(AccountConstants.DefaultRole);
            }

            var unknownRoles = requestedRoles
                .Where(role => !existingRoles.Contains(role))
                .ToArray();

            if (unknownRoles.Length > 0)
            {
                return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    [nameof(UpdateIdentityUserRolesRequest.Roles)] = unknownRoles
                }));
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles
                .Where(role => !requestedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                .ToArray();
            var rolesToAdd = requestedRoles
                .Where(role => !currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            var removeResult = rolesToRemove.Length == 0
                ? IdentityResult.Success
                : await userManager.RemoveFromRolesAsync(user, rolesToRemove);

            if (!removeResult.Succeeded)
            {
                return BadRequest(new ValidationProblemDetails(ToValidationErrors(removeResult)));
            }

            var addResult = rolesToAdd.Length == 0
                ? IdentityResult.Success
                : await userManager.AddToRolesAsync(user, rolesToAdd);

            if (!addResult.Succeeded)
            {
                return BadRequest(new ValidationProblemDetails(ToValidationErrors(addResult)));
            }

            user.ModifiedUtc = DateTime.UtcNow;
            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return BadRequest(new ValidationProblemDetails(ToValidationErrors(updateResult)));
            }

            var details = JsonSerializer.Serialize(new { Roles = requestedRoles.Order(StringComparer.OrdinalIgnoreCase).ToArray() });
            await securityEvents.Record(AccountConstants.Actions.RolesUpdated, user.Id, detailsJson: details, cancellationToken: cancellationToken);

            return NoContent();
        }

        private bool HasAdminApiKey()
        {
            var currentOptions = options.Value;

            if (!currentOptions.Enabled ||
                string.IsNullOrWhiteSpace(currentOptions.ApiKeyHeaderName) ||
                string.IsNullOrWhiteSpace(currentOptions.ApiKey) ||
                !Request.Headers.TryGetValue(currentOptions.ApiKeyHeaderName, out var values))
            {
                return false;
            }

            var provided = values.FirstOrDefault();

            return !string.IsNullOrWhiteSpace(provided) &&
                FixedTimeEquals(provided.Trim(), currentOptions.ApiKey);
        }

        private async Task<IdentityAdminUserResponse> MapUser(IdentityUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            var locked = await userManager.IsLockedOutAsync(user);

            return new(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                !locked,
                locked,
                roles.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                user.CreatedUtc,
                user.LockoutEnd);
        }

        private static IQueryable<IdentityUser> ApplySorting(
            IQueryable<IdentityUser> query,
            string? sortBy,
            bool ascending)
        {
            if (string.Equals(sortBy, nameof(IdentityAdminUserResponse.Email), StringComparison.OrdinalIgnoreCase))
            {
                return ascending ? query.OrderBy(user => user.Email) : query.OrderByDescending(user => user.Email);
            }

            if (string.Equals(sortBy, nameof(IdentityAdminUserResponse.CreatedUtc), StringComparison.OrdinalIgnoreCase))
            {
                return ascending ? query.OrderBy(user => user.CreatedUtc) : query.OrderByDescending(user => user.CreatedUtc);
            }

            return ascending ? query.OrderBy(user => user.UserName) : query.OrderByDescending(user => user.UserName);
        }

        private static IdentityAdminPagedResponse<T> CreatePage<T>(
            IReadOnlyList<T> collection,
            long total,
            int page,
            int pageSize)
        {
            var totalPages = total > 0 && pageSize > 0
                ? (int)Math.Ceiling(decimal.Divide(total, pageSize))
                : 0;

            return new(collection, total, page, pageSize, totalPages);
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left);
            var rightBytes = Encoding.UTF8.GetBytes(right);

            return leftBytes.Length == rightBytes.Length &&
                CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }

        private static Dictionary<string, string[]> ToValidationErrors(IdentityResult result)
        {
            return result.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.Description).ToArray());
        }
    }
}
