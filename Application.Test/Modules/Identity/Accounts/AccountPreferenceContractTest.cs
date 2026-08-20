using System.Text.Json;
using Domain.Modules.Identity;
using Application.Modules.Identity.Accounts.Preferences.Commands;

namespace Application.Test.Modules.Identity.Accounts
{
    public sealed class AccountPreferenceContractTest
    {
        [Theory]
        [InlineData(AccountTheme.Dark, "\"dark\"")]
        [InlineData(AccountTheme.Light, "\"light\"")]
        public void AccountTheme_Should_Match_The_Frontend_String_Contract(AccountTheme theme, string expected)
        {
            Assert.Equal(expected, JsonSerializer.Serialize(theme));
            Assert.Equal(theme, JsonSerializer.Deserialize<AccountTheme>(expected));
        }

        [Fact]
        public void Save_Command_Should_Not_Expose_AccountId_In_Json()
        {
            var json = JsonSerializer.Serialize(new SaveAccountPreferencesCommand
            {
                AccountId = Guid.NewGuid(),
                Culture = "en-US",
                Theme = AccountTheme.Dark,
                TimeZone = "America/Hermosillo"
            });

            Assert.DoesNotContain("AccountId", json, StringComparison.OrdinalIgnoreCase);
        }
    }
}
