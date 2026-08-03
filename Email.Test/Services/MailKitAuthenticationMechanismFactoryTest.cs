using Email.Services;
using MailKit.Security;
using Email.Configuration;

namespace Email.Test.Services
{
    public sealed class MailKitAuthenticationMechanismFactoryTest
    {
        [Fact]
        public void Create_Should_Select_Login_Mechanism()
        {
            var mechanism = CreateFactory().Create(CreateOptions(EmailAuthenticationType.Login));

            Assert.IsType<SaslMechanismLogin>(mechanism);
        }

        [Fact]
        public void Create_Should_Select_Plain_Mechanism()
        {
            var mechanism = CreateFactory().Create(CreateOptions(EmailAuthenticationType.Plain));

            Assert.IsType<SaslMechanismPlain>(mechanism);
        }

        [Fact]
        public void Create_Should_Select_OAuth2_Mechanism()
        {
            var mechanism = CreateFactory().Create(CreateOptions(EmailAuthenticationType.OAuth));

            Assert.IsType<SaslMechanismOAuth2>(mechanism);
        }

        [Fact]
        public void Create_Should_Reject_Anonymous_Authentication()
        {
            var options = CreateOptions(EmailAuthenticationType.Anonymous);

            Assert.Throws<InvalidOperationException>(() => CreateFactory().Create(options));
        }

        private static MailKitAuthenticationMechanismFactory CreateFactory()
        {
            return new MailKitAuthenticationMechanismFactory();
        }

        private static EmailAuthenticationOptions CreateOptions(EmailAuthenticationType type)
        {
            return new EmailAuthenticationOptions
            {
                Type = type,
                Login = new EmailUsernamePasswordOptions
                {
                    Username = "login-user",
                    Password = "login-password"
                },
                Plain = new EmailUsernamePasswordOptions
                {
                    Username = "plain-user",
                    Password = "plain-password"
                },
                OAuth = new EmailOAuthOptions
                {
                    Username = "oauth-user",
                    AccessToken = "oauth-access-token"
                }
            };
        }
    }
}
