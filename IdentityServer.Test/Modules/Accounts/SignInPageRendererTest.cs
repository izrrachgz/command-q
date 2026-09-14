using IdentityServer.Modules.Accounts;
using IdentityServer.Localization;
using IdentityServer.Resources;
using IdentityServer.Test.Constants;
using IdentityServer.Modules.Accounts.Configuration;
using IdentityServer.Modules.Accounts.Services.SignIn;
using Microsoft.Extensions.Options;

namespace IdentityServer.Test.Modules.Accounts
{
    public sealed class SignInPageRendererTest
    {
        [Fact]
        public void Render_Should_Use_Template_And_Url_Encode_Nested_Authorization_ReturnUrl()
        {
            string html = CreateRenderer().Render(IdentityServerTestConstants.LocalAuthorizationReturnUrl);

            Assert.Contains(
                $"action=\"{AccountConstants.Routes.SignInPath}?{AccountConstants.ReturnUrlParameter}={Uri.EscapeDataString(IdentityServerTestConstants.LocalAuthorizationReturnUrl)}\"",
                html);
            Assert.DoesNotContain(IdentityServerTestConstants.LocalAuthorizationReturnUrl, html);
            Assert.DoesNotContain("{{", html);
            Assert.DoesNotContain("}}", html);
        }

        [Fact]
        public void Render_Should_Show_Authentication_Failure_When_Requested()
        {
            string html = CreateRenderer().Render(AccountConstants.DefaultReturnUrl, failed: true);

            Assert.Contains("""<p class="alert">""", html);
            Assert.Contains("The username or password is invalid.", html);
            Assert.DoesNotContain("{{", html);
        }

        [Fact]
        public void Format_Should_Apply_Resource_Arguments()
        {
            string text = IdentityServerText.Format(
                IdentityServerMessages.Configuration_Required,
                "IdentityServer:OpenIddict:ApiScope");

            Assert.Equal("IdentityServer:OpenIddict:ApiScope is required.", text);
        }

        private static SignInPageRenderer CreateRenderer()
        {
            return new SignInPageRenderer(Options.Create(new AccountEmailOptions
            {
                ApplicationBaseUrl = "http://localhost:4200"
            }));
        }
    }
}
