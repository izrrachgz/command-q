using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Modules.Accounts.Requests
{
    /// <summary>Defines filtering, sorting, and paging values for an identity administration search.</summary>
    public sealed record IdentityAdminSearchCriteria : IValidatableObject
    {
        /// <summary>Gets the inclusive UTC beginning of the account creation-time filter.</summary>
        public DateTimeOffset? StartDate { get; init; }
        /// <summary>Gets the inclusive UTC end of the account creation-time filter.</summary>
        public DateTimeOffset? EndDate { get; init; }

        /// <summary>Gets the optional username or email search text.</summary>
        public string? SearchTerm { get; init; }
        /// <summary>Gets the one-based page number.</summary>
        public int Page { get; init; } = 1;
        /// <summary>Gets the requested page size.</summary>
        public int Size { get; init; } = 25;
        /// <summary>Gets the supported response property used for sorting.</summary>
        public string? SortBy { get; init; }
        /// <summary>Gets whether results are sorted in ascending order.</summary>
        public bool AscendingSort { get; init; } = true;

        /// <inheritdoc />
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

    /// <summary>Contains the complete role assignment requested for an identity user.</summary>
    public sealed record UpdateIdentityUserRolesRequest
    {
        /// <summary>Gets the complete set of roles that should remain assigned to the user.</summary>
        [Required]
        public IReadOnlyList<string> Roles { get; init; } = [];
    }
}
