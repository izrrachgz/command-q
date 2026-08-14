using Email.Services;
using Email.Constants;
using Email.Abstractions;
using Email.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Email.Extensions
{
    /// <summary>
    /// Registers MailKit-backed email delivery services.
    /// </summary>
    public static class EmailServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var layeredConfiguration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(
                    EmailConstants.DefaultSettingsPath,
                    optional: true,
                    reloadOnChange: true)
                .AddConfiguration(configuration)
                .Build();

            return services.AddEmailServices(layeredConfiguration.GetRequiredSection(
                EmailConstants.ConfigurationSection));
        }

        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfigurationSection section)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(section);

            services.AddOptions<EmailOptions>()
                .Bind(section)
                .ValidateOnStart();

            return AddEmailCore(services);
        }

        public static IServiceCollection AddEmailServices(this IServiceCollection services, Action<EmailOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            services.AddOptions<EmailOptions>()
                .Configure(configure)
                .ValidateOnStart();

            return AddEmailCore(services);
        }

        private static IServiceCollection AddEmailCore(IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>());
            services.TryAddSingleton<MailKitEmailSenderFactory>();
            services.TryAddSingleton<IEmailSenderFactory>(services => services.GetRequiredService<MailKitEmailSenderFactory>());
            services.TryAddSingleton<IEmailSender>(services => services.GetRequiredService<IEmailSenderFactory>().Create());
            services.TryAddSingleton<ISmtpTransportFactory, MailKitSmtpTransportFactory>();
            services.TryAddSingleton<MailKitAuthenticationMechanismFactory>();
            services.TryAddSingleton<MimeMessageFactory>();
            services.TryAddSingleton<EmailMessageValidator>();

            return services;
        }
    }
}
