using OpenIddict.Server;
using OpenIddict.Abstractions;
using IdentityServer.Constants;
using System.Security.Cryptography.X509Certificates;
using IdentityServer.Modules.OpenIddict.Configuration;

namespace IdentityServer.Modules.OpenIddict
{
    internal static class OpenIddictModuleExtensions
    {
        public static IServiceCollection AddOpenIddictModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            var serverOptions = GetAndValidateOptions(configuration, environment);
            services.AddOptions<AuthorizationServerOptions>()
                .Bind(configuration.GetRequiredSection(ConfigKeys.OpenIddict.Section))
                .ValidateOnStart();

            services.AddOpenIddict()
                .AddServer(options => ConfigureServer(options, serverOptions, environment));

            return services;
        }

        private static void ConfigureServer(
            OpenIddictServerBuilder options,
            AuthorizationServerOptions serverOptions,
            IHostEnvironment environment)
        {
            options.SetIssuer(new Uri(serverOptions.Issuer, UriKind.Absolute));
            options.SetAuthorizationEndpointUris(OpenIddictRoutes.Authorization);
            options.SetEndSessionEndpointUris(OpenIddictRoutes.EndSession);
            options.SetRevocationEndpointUris(OpenIddictRoutes.Revocation);
            options.SetTokenEndpointUris(OpenIddictRoutes.Token);
            options.SetUserInfoEndpointUris(OpenIddictRoutes.UserInfo);

            options.AllowAuthorizationCodeFlow();
            options.AllowClientCredentialsFlow();
            options.AllowRefreshTokenFlow();
            options.RequireProofKeyForCodeExchange();

            options.RegisterScopes(
                OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Scopes.Roles,
                serverOptions.ApiScope);

            options.DisableAccessTokenEncryption();

            options.SetAccessTokenLifetime(TimeSpan.FromMinutes(serverOptions.AccessTokenMinutes));
            options.SetRefreshTokenLifetime(TimeSpan.FromDays(serverOptions.RefreshTokenDays));

            ConfigureCertificates(options, serverOptions.Certificates, environment);

            options.UseAspNetCore()
                .EnableAuthorizationEndpointPassthrough()
                .EnableEndSessionEndpointPassthrough()
                .EnableTokenEndpointPassthrough()
                .EnableUserInfoEndpointPassthrough();
        }

        private static void ConfigureCertificates(
            OpenIddictServerBuilder options,
            CertificateOptions certificates,
            IHostEnvironment environment)
        {
            if (certificates.UseDevelopmentCertificates)
            {
                options.AddDevelopmentEncryptionCertificate();
                options.AddDevelopmentSigningCertificate();
                return;
            }

            options.AddSigningCertificate(LoadCertificate(certificates.Signing, environment));
            options.AddEncryptionCertificate(LoadCertificate(certificates.Encryption, environment));
        }

        private static X509Certificate2 LoadCertificate(CertificateFileOptions certificate, IHostEnvironment environment)
        {
            string path = Path.IsPathRooted(certificate.Path)
                ? certificate.Path
                : Path.Combine(environment.ContentRootPath, certificate.Path);

            return X509CertificateLoader.LoadPkcs12FromFile(path, certificate.Password);
        }

        private static AuthorizationServerOptions GetAndValidateOptions(IConfiguration configuration, IHostEnvironment environment)
        {
            var options = configuration
                .GetRequiredSection(ConfigKeys.OpenIddict.Section)
                .Get<AuthorizationServerOptions>()
                ?? throw new InvalidOperationException("OpenIddict configuration is required.");

            if (!Uri.TryCreate(options.Issuer, UriKind.Absolute, out var issuer) ||
                !string.Equals(issuer.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("IdentityServer:OpenIddict:Issuer must be an absolute HTTPS URI.");
            }

            if (options.AccessTokenMinutes <= 0 || options.RefreshTokenDays <= 0)
            {
                throw new InvalidOperationException("OpenIddict token lifetimes must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(options.ApiScope))
            {
                throw new InvalidOperationException("IdentityServer:OpenIddict:ApiScope is required.");
            }

            if (options.Clients.Any(client =>
                string.IsNullOrWhiteSpace(client.ClientId) ||
                string.IsNullOrWhiteSpace(client.DisplayName) ||
                client.RedirectUris.Any(uri => !Uri.TryCreate(uri, UriKind.Absolute, out _)) ||
                client.PostLogoutRedirectUris.Any(uri => !Uri.TryCreate(uri, UriKind.Absolute, out _)) ||
                client.AllowClientCredentials && string.IsNullOrWhiteSpace(client.ClientSecret)))
            {
                throw new InvalidOperationException("OpenIddict client configuration is invalid.");
            }

            if (options.Certificates.UseDevelopmentCertificates && !environment.IsDevelopment())
            {
                throw new InvalidOperationException("OpenIddict development certificates can only be used in Development.");
            }

            if (!options.Certificates.UseDevelopmentCertificates &&
                (string.IsNullOrWhiteSpace(options.Certificates.Signing.Path) ||
                 string.IsNullOrWhiteSpace(options.Certificates.Encryption.Path)))
            {
                throw new InvalidOperationException("OpenIddict signing and encryption certificate paths are required outside local Development.");
            }

            return options;
        }

    }
}
