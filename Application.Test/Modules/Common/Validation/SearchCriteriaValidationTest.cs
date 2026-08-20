using FluentValidation;
using Common.Models.Request;
using Application.Modules.Identity.Roles.Queries;

namespace Application.Test.Modules.Common.Validation
{
    public sealed class SearchCriteriaValidationTest(IValidator<SearchIdentityUsersQuery> validator)
    {
        private readonly IValidator<SearchIdentityUsersQuery> _validator = validator;

        [Fact]
        public async Task SearchCriteria_Should_Accept_Complete_Utc_Range_And_Tzdb_TimeZone()
        {
            var query = CreateQuery(
                new DateTimeOffset(2026, 8, 17, 18, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 8, 18, 18, 0, 0, TimeSpan.Zero),
                "America/Hermosillo");

            var result = await _validator.ValidateAsync(query, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task SearchCriteria_Should_Reject_NonUtc_Offsets()
        {
            var query = CreateQuery(
                new DateTimeOffset(2026, 8, 17, 18, 0, 0, TimeSpan.FromHours(-7)),
                new DateTimeOffset(2026, 8, 18, 18, 0, 0, TimeSpan.Zero),
                "America/Hermosillo");

            var result = await _validator.ValidateAsync(query, TestContext.Current.CancellationToken);

            Assert.Contains(result.Errors, failure => failure.PropertyName.EndsWith(nameof(SearchCriteria.StartDate)));
        }

        [Fact]
        public async Task SearchCriteria_Should_Require_Complete_Range_And_TimeZone()
        {
            var query = CreateQuery(
                new DateTimeOffset(2026, 8, 17, 18, 0, 0, TimeSpan.Zero),
                null,
                null);

            var result = await _validator.ValidateAsync(query, TestContext.Current.CancellationToken);

            Assert.Contains(result.Errors, failure => failure.PropertyName.EndsWith(nameof(SearchCriteria.EndDate)));
            Assert.Contains(result.Errors, failure => failure.PropertyName.EndsWith(nameof(SearchCriteria.TimeZone)));
        }

        [Fact]
        public async Task SearchCriteria_Should_Reject_Reversed_Range_And_Invalid_TimeZone()
        {
            var query = CreateQuery(
                new DateTimeOffset(2026, 8, 18, 18, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 8, 17, 18, 0, 0, TimeSpan.Zero),
                "Invalid/TimeZone");

            var result = await _validator.ValidateAsync(query, TestContext.Current.CancellationToken);

            Assert.Contains(result.Errors, failure => failure.PropertyName.EndsWith(nameof(SearchCriteria.EndDate)));
            Assert.Contains(result.Errors, failure => failure.PropertyName.EndsWith(nameof(SearchCriteria.TimeZone)));
        }

        private static SearchIdentityUsersQuery CreateQuery(
            DateTimeOffset? startDate,
            DateTimeOffset? endDate,
            string? timeZone)
        {
            return new()
            {
                SearchCriteria = new()
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TimeZone = timeZone
                }
            };
        }
    }
}
