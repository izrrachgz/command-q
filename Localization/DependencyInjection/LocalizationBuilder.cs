using System.Resources;
using Localization.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Localization.DependencyInjection
{
    /// <summary>
    /// Registers application-owned localization sources with the shared localization mechanism.
    /// </summary>
    public sealed class LocalizationBuilder
    {
        internal LocalizationBuilder(IServiceCollection services)
        {
            Services = services;
        }

        /// <summary>
        /// Gets the service collection being configured.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Adds an application-owned RESX resource catalog.
        /// </summary>
        public LocalizationBuilder AddResx(ResourceManager resourceManager)
        {
            ArgumentNullException.ThrowIfNull(resourceManager);
            Services.AddSingleton(new ResxLocalizationSourceRegistration(resourceManager));
            return this;
        }

        /// <summary>
        /// Adds application-owned JSON locale files. The configured
        /// <see cref="LocalizationOptions.JsonLocalesPath"/> is used when <paramref name="localesPath"/> is omitted.
        /// Relative paths are resolved from <see cref="AppContext.BaseDirectory"/>. Files may use exact
        /// culture names such as en-US.json or neutral names such as en.json.
        /// </summary>
        public LocalizationBuilder AddJsonFiles(string? localesPath = null)
        {
            Services.AddSingleton(new JsonLocalizationSourceRegistration(localesPath));
            return this;
        }
    }
}
