using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Modules.Accounts
{
    public sealed record IdentityAdminSearchCriteria : IValidatableObject
    {
        public DateTimeOffset? StartDate { get; init; }
        public DateTimeOffset? EndDate { get; init; }

        public string? SearchTerm { get; init; }
        public int Page { get; init; } = 1;
        public int Size { get; init; } = 25;
        public string? SortBy { get; init; }
        public bool AscendingSort { get; init; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate.HasValue != EndDate.HasValue)
            {
                yield return new ValidationResult(
                    "StartDate and EndDate must be provided together.",
                    [nameof(StartDate), nameof(EndDate)]);
            }

            if (StartDate.HasValue && StartDate.Value.Offset != TimeSpan.Zero)
            {
                yield return new ValidationResult("StartDate must be UTC.", [nameof(StartDate)]);
            }

            if (EndDate.HasValue && EndDate.Value.Offset != TimeSpan.Zero)
            {
                yield return new ValidationResult("EndDate must be UTC.", [nameof(EndDate)]);
            }

            if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
            {
                yield return new ValidationResult(
                    "EndDate cannot be before StartDate.",
                    [nameof(EndDate)]);
            }
        }
    }

    public sealed record IdentityAdminRoleResponse(Guid Id, string Name);

    public sealed record IdentityAdminUserResponse(
        Guid Id,
        string Username,
        string Email,
        bool Active,
        bool Locked,
        IReadOnlyList<string> Roles,
        DateTime CreatedUtc,
        DateTimeOffset? LockedUntilUtc);

    public sealed record IdentityAdminPagedResponse<T>(
        IReadOnlyList<T> Collection,
        long Total,
        int Page,
        int PageSize,
        int TotalPages);

    public sealed record UpdateIdentityUserRolesRequest
    {
        [Required]
        public IReadOnlyList<string> Roles { get; init; } = [];
    }
}
