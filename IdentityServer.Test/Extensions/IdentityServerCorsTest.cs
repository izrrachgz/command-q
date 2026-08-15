using IdentityServer.Configuration;
using IdentityServer.Extensions;
using IdentityServer.Test.Constants;

namespace IdentityServer.Test.Extensions
{
    public sealed class IdentityServerCorsTest
    {
        [Fact]
        public void Allowed_Cors_Origin_Should_Require_Configured_Origins()
        {
            var options = new CorsOptions
            {
                AllowedOrigins = [
                    IdentityServerTestConstants.LocalSpaOrigin,
                    $"{IdentityServerTestConstants.LocalLoopbackSpaOrigin}/"
                ]
            };

            Assert.True(ProgramExtensions.IsAllowedCorsOrigin(
                IdentityServerTestConstants.LocalSpaOrigin,
                options,
                isDevelopment: true));
            Assert.True(ProgramExtensions.IsAllowedCorsOrigin(
                IdentityServerTestConstants.LocalLoopbackSpaOrigin,
                options,
                isDevelopment: true));
            Assert.False(ProgramExtensions.IsAllowedCorsOrigin(
                IdentityServerTestConstants.LocalRejectedSpaOrigin,
                options,
                isDevelopment: true));
            Assert.False(ProgramExtensions.IsAllowedCorsOrigin(
                IdentityServerTestConstants.ExternalAppOrigin,
                options,
                isDevelopment: true));
        }

        [Fact]
        public void Cors_Configuration_Should_Be_Closed_By_Default_Outside_Development()
        {
            var disabled = new CorsOptions
            {
                AllowedOrigins = [IdentityServerTestConstants.ExternalAppOrigin]
            };
            var enabled = disabled with { Enabled = true };

            Assert.False(ProgramExtensions.IsAllowedCorsOrigin(
                IdentityServerTestConstants.ExternalAppOrigin,
                disabled,
                isDevelopment: false));
            Assert.True(ProgramExtensions.IsAllowedCorsOrigin(
                IdentityServerTestConstants.ExternalAppOrigin,
                enabled,
                isDevelopment: false));
        }

        [Fact]
        public void Cors_Configuration_Should_Reject_Invalid_Origins_When_Enabled()
        {
            var options = new CorsOptions
            {
                Enabled = true,
                AllowedOrigins = [IdentityServerTestConstants.ExternalAppCallbackUri]
            };

            Assert.False(ProgramExtensions.IsValidCorsConfiguration(options, isDevelopment: false));
        }
    }
}
