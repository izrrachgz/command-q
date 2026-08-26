using NodaTime;
using Application.Modules.Identity.Accounts.Preferences;

namespace Application.Test.Modules.Identity.Accounts.Preferences
{
    public sealed class AccountPreferenceConstantsTest
    {
        [Fact]
        public void TimeZone_MaxLength_Should_Accommodate_All_Current_Tzdb_Ids()
        {
            int longestIdentifier = DateTimeZoneProviders.Tzdb.Ids.Max(identifier => identifier.Length);

            Assert.True(longestIdentifier <= AccountPreferenceConstants.TimeZoneMaxLength);
        }
    }
}
