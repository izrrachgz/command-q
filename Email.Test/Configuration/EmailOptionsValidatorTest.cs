using Email.Configuration;

namespace Email.Test.Configuration
{
    public sealed class EmailOptionsValidatorTest
    {
        [Fact]
        public void Validate_Should_Accept_Complete_Secure_Configuration()
        {
            var validator = new EmailOptionsValidator();
            var options = CreateOptions(EmailAuthenticationType.Login);

            var result = validator.Validate(null, options);

            Assert.True(result.Succeeded);
        }

        [Fact]
        public void Validate_Should_Allow_Incomplete_Configuration_When_Email_Is_Disabled()
        {
            var validator = new EmailOptionsValidator();
            var options = new EmailOptions();

            var result = validator.Validate(null, options);

            Assert.True(result.Succeeded);
        }

        [Fact]
        public void Validate_Should_Reject_Partial_Credentials()
        {
            var validator = new EmailOptionsValidator();
            var options = CreateOptions(EmailAuthenticationType.Plain);
            options.Authentication.Plain.Password = string.Empty;

            var result = validator.Validate(null, options);

            Assert.True(result.Failed);
            Assert.Contains(result.Failures, failure => failure.Contains("Plain:Password", StringComparison.Ordinal));
        }

        [Fact]
        public void Validate_Should_Allow_Anonymous_Delivery_Without_Credentials()
        {
            var validator = new EmailOptionsValidator();
            var options = CreateOptions(EmailAuthenticationType.Anonymous);
            options.Authentication.Login = new EmailUsernamePasswordOptions();
            options.Authentication.Plain = new EmailUsernamePasswordOptions();
            options.Authentication.OAuth = new EmailOAuthOptions();

            var result = validator.Validate(null, options);

            Assert.True(result.Succeeded);
        }

        [Fact]
        public void Validate_Should_Require_Access_Token_For_OAuth()
        {
            var validator = new EmailOptionsValidator();
            var options = CreateOptions(EmailAuthenticationType.OAuth);
            options.Authentication.OAuth.AccessToken = string.Empty;

            var result = validator.Validate(null, options);

            Assert.True(result.Failed);
            Assert.Contains(result.Failures, failure => failure.Contains("OAuth:AccessToken", StringComparison.Ordinal));
        }

        [Fact]
        public void Validate_Should_Reject_Authenticated_Delivery_Without_Required_Tls()
        {
            var validator = new EmailOptionsValidator();
            var options = CreateOptions(EmailAuthenticationType.Login);
            options.Smtp.Security = EmailSecurityMode.StartTlsWhenAvailable;

            var result = validator.Validate(null, options);

            Assert.True(result.Failed);
            Assert.Contains(result.Failures, failure => failure.Contains("requires", StringComparison.Ordinal));
        }

        private static EmailOptions CreateOptions(EmailAuthenticationType authenticationType)
        {
            return new EmailOptions
            {
                Enabled = true,
                Smtp = new EmailSmtpOptions
                {
                    Host = "smtp.example.test",
                    Port = 587,
                    Security = EmailSecurityMode.StartTls
                },
                Authentication = new EmailAuthenticationOptions
                {
                    Type = authenticationType,
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
                },
                Sender = new EmailSenderOptions
                {
                    Address = "sender@example.test",
                    Name = "Example Sender"
                }
            };
        }
    }
}
