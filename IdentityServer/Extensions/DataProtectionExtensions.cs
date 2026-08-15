using IdentityServer.Constants;
using Microsoft.AspNetCore.DataProtection;
using IdentityServer.Localization;
using IdentityServer.Resources;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer.Extensions
{
    internal static class DataProtectionExtensions
    {
        public static IServiceCollection AddIdentityServerDataProtection(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            var options = configuration
                .GetRequiredSection(IdentityServerConfigKeys.DataProtection.Section)
                .Get<DataProtectionConfigurationOptions>()
                ?? throw new InvalidOperationException(
                    IdentityServerMessages.Configuration_DataProtectionRequired);

            if (string.IsNullOrWhiteSpace(options.ApplicationName))
            {
                throw new InvalidOperationException(
                    Required(IdentityServerConfigKeys.DataProtection.ApplicationName));
            }

            var keysPath = options.KeysPath;

            if (string.IsNullOrWhiteSpace(keysPath))
            {
                if (!environment.IsDevelopment())
                {
                    throw new InvalidOperationException(
                        RequiredOutsideDevelopment(IdentityServerConfigKeys.DataProtection.KeysPath));
                }

                keysPath = Path.Combine(environment.ContentRootPath, "App_Data", "DataProtectionKeys");
            }
            else if (!Path.IsPathRooted(keysPath))
            {
                keysPath = Path.Combine(environment.ContentRootPath, keysPath);
            }

            var builder = services.AddDataProtection()
                .SetApplicationName(options.ApplicationName)
                .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

            if (!environment.IsDevelopment())
            {
                if (string.IsNullOrWhiteSpace(options.Certificate.Path))
                {
                    throw new InvalidOperationException(
                        RequiredOutsideDevelopment(IdentityServerConfigKeys.DataProtection.Certificate.Path));
                }

                var certificatePath = Path.IsPathRooted(options.Certificate.Path)
                    ? options.Certificate.Path
                    : Path.Combine(environment.ContentRootPath, options.Certificate.Path);

                builder.ProtectKeysWithCertificate(
                    X509CertificateLoader.LoadPkcs12FromFile(certificatePath, options.Certificate.Password));
            }

            return services;
        }

        private sealed class DataProtectionConfigurationOptions
        {
            public string ApplicationName { get; init; } = string.Empty;
            public string KeysPath { get; init; } = string.Empty;
            public CertificateConfigurationOptions Certificate { get; init; } = new();
        }

        private sealed class CertificateConfigurationOptions
        {
            public string Path { get; init; } = string.Empty;
            public string Password { get; init; } = string.Empty;
        }

        private static string Required(string childPath)
        {
            return IdentityServerText.Format(
                IdentityServerMessages.Configuration_Required,
                IdentityServerConfigKeys.FullPath(childPath));
        }

        private static string RequiredOutsideDevelopment(string childPath)
        {
            return IdentityServerText.Format(
                IdentityServerMessages.Configuration_RequiredOutsideDevelopment,
                IdentityServerConfigKeys.FullPath(childPath));
        }
    }
}
