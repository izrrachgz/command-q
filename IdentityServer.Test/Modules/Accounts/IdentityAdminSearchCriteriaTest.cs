using IdentityServer.Modules.Accounts;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Test.Modules.Accounts
{
    public sealed class IdentityAdminSearchCriteriaTest
    {
        [Fact]
        public void Validate_Should_Accept_Complete_Utc_Range()
        {
            var criteria = new IdentityAdminSearchCriteria
            {
                StartDate = new DateTimeOffset(2026, 8, 17, 18, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2026, 8, 18, 18, 0, 0, TimeSpan.Zero),
                TimeZone = "America/Hermosillo"
            };

            var results = Validate(criteria);

            Assert.Empty(results);
        }

        [Fact]
        public void Validate_Should_Reject_Incomplete_NonUtc_Range_Without_TimeZone()
        {
            var criteria = new IdentityAdminSearchCriteria
            {
                StartDate = new DateTimeOffset(2026, 8, 17, 18, 0, 0, TimeSpan.FromHours(-7))
            };

            var results = Validate(criteria);

            Assert.Contains(results, result => result.MemberNames.Contains(nameof(IdentityAdminSearchCriteria.EndDate)));
            Assert.Contains(results, result => result.MemberNames.Contains(nameof(IdentityAdminSearchCriteria.StartDate)));
            Assert.Contains(results, result => result.MemberNames.Contains(nameof(IdentityAdminSearchCriteria.TimeZone)));
        }

        private static IReadOnlyList<ValidationResult> Validate(IdentityAdminSearchCriteria criteria)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(criteria, new ValidationContext(criteria), results, validateAllProperties: true);
            return results;
        }
    }
}
