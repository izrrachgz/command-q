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
        public void Validate_Should_Accept_Complete_Named_Profiles()
        {
            var validator = new EmailOptionsValidator();
            var profile = new EmailProfileOptions
            {
                Smtp = new EmailSmtpOptions
                {
                    Host = "smtp.example.test",
                    Port = 587,
                    Security = EmailSecurityMode.StartTls
                },
                Sender = new EmailSenderOptions { Address = "sender@example.test" }
            };
            var options = new EmailOptions
            {
                Enabled = true,
                DefaultProfile = "Transactional",
                Profiles = new Dictionary<string, EmailProfileOptions>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Transactional"] = profile
                }
            };

            var result = validator.Validate(null, options);

            Assert.True(result.Succeeded);
        }

        [Fact]
        public void ResolveProfile_Should_Map_Durable_Default_Marker_To_Configured_Default()
        {
            var options = new EmailOptions
            {
                DefaultProfile = "Transactional",
                Profiles = new Dictionary<string, EmailProfileOptions>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Transactional"] = new EmailProfileOptions()
                }
            };

            (string name, EmailProfileOptions _) = options.ResolveProfile("default");

            Assert.Equal("Transactional", name);
        }

        [Fact]
        public void ResolveProfile_Should_Return_Configured_Canonical_Name()
        {
            var options = new EmailOptions
            {
                DefaultProfile = "Transactional",
                Profiles = new Dictionary<string, EmailProfileOptions>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Transactional"] = new EmailProfileOptions()
                }
            };

            (string name, EmailProfileOptions _) = options.ResolveProfile("transactional");

            Assert.Equal("Transactional", name);
        }

        [Fact]
        public void Validate_Should_Reject_Profile_Name_Whitespace()
        {
            var validator = new EmailOptionsValidator();
            var options = new EmailOptions
            {
                Enabled = true,
                DefaultProfile = " Transactional ",
                Profiles = new Dictionary<string, EmailProfileOptions>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Transactional"] = new EmailProfileOptions { Enabled = false }
                }
            };

            var result = validator.Validate(null, options);

            Assert.True(result.Failed);
            Assert.Contains(result.Failures, failure => failure.Contains("whitespace", StringComparison.Ordinal));
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

        [Fact]
        public void Validate_Should_Reject_Undeclared_Enum_Configuration_Values()
        {
            var validator = new EmailOptionsValidator();
            var options = CreateOptions(EmailAuthenticationType.Anonymous);
            options.Smtp.Security = (EmailSecurityMode)99;
            options.Authentication.Type = (EmailAuthenticationType)99;

            var result = validator.Validate(null, options);

            Assert.True(result.Failed);
            Assert.Contains(result.Failures, failure => failure.Contains("Smtp:Security", StringComparison.Ordinal));
            Assert.Contains(result.Failures, failure => failure.Contains("Authentication:Type", StringComparison.Ordinal));
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
