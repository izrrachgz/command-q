using System.Text.Json;
using Domain.Modules.Identity;

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
    }
}
