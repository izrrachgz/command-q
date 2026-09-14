using IdentityServer.Modules.Accounts;
using IdentityServer.Modules.Accounts.Services.Email;

namespace IdentityServer.Test.Modules.Accounts
{
    public sealed class AccountEmailTemplateRendererTest
    {
        [Theory]
        [InlineData("en-US", "Confirm your account")]
        [InlineData("es-ES", "Confirma tu cuenta")]
        public void Confirmation_Template_Should_Use_Shared_Layout_And_Encode_Values(string culture, string heading)
        {
            var renderer = new AccountEmailTemplateRenderer();

            var content = renderer.RenderConfirmation(
                "<unsafe>",
                "123456",
                "https://example.test/auth/confirm-email#token=a%26b%3Dc",
                new DateTime(2026, 9, 13, 1, 0, 0, DateTimeKind.Utc),
                culture);

            Assert.Contains(heading, content.HtmlBody);
            Assert.Contains("command-q", content.HtmlBody);
            Assert.Contains("123456", content.HtmlBody);
            Assert.Contains("&lt;unsafe&gt;", content.HtmlBody);
            Assert.DoesNotContain("<unsafe>", content.HtmlBody);
            Assert.Contains("a%26b%3Dc", content.HtmlBody);
            Assert.DoesNotContain("{{", content.HtmlBody);
            Assert.DoesNotContain("{{", content.TextBody);
            Assert.Contains(culture == "es-ES" ? "tu cuenta no será bloqueada" : "your account will not be locked", content.TextBody);
        }

        [Fact]
        public void Password_Recovery_Template_Should_Explain_Three_Attempt_Lockout()
        {
            var renderer = new AccountEmailTemplateRenderer();

            var content = renderer.RenderPasswordRecovery(
                "test-user",
                "654321",
                "https://example.test/auth/recover-password#token=value",
                new DateTime(2026, 9, 13, 1, 0, 0, DateTimeKind.Utc),
                "en-US");

            Assert.Contains("Each stage has its own attempt limit", content.HtmlBody);
            Assert.Contains("Each stage has its own attempt limit", content.TextBody);
        }
    }
}
