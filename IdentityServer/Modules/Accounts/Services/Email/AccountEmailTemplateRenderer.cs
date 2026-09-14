using System.Globalization;
using IdentityServer.Modules.Accounts.Abstractions.Email;
using IdentityServer.Modules.Accounts.Contracts.Email;
using System.Net;
using System.Reflection;

namespace IdentityServer.Modules.Accounts.Services.Email
{
    /// <summary>Renders localized account-confirmation and password-recovery templates using shared embedded layouts.</summary>
    /// <remarks>User-controlled substitutions are HTML encoded before being inserted into HTML templates.</remarks>
    internal sealed class AccountEmailTemplateRenderer : IAccountEmailTemplateRenderer
    {
        private const string ResourcePrefix = "IdentityServer.Modules.Accounts.Templates.Email";
        private const string ContentPlaceholder = "{{content}}";
        private static readonly Assembly Assembly = typeof(AccountEmailTemplateRenderer).Assembly;

        /// <inheritdoc />
        public AccountEmailContent RenderConfirmation(
            string username,
            string code,
            string actionUrl,
            DateTime expiresUtc,
            string? culture)
        {
            var language = ResolveLanguage(culture);
            var subject = language == AccountConstants.Cultures.Spanish
                ? "Confirma tu cuenta de command-q"
                : "Confirm your command-q account";
            var values = CreateValues(username, code, actionUrl, expiresUtc, language);

            return Render("confirmation", subject, language, values);
        }

        /// <inheritdoc />
        public AccountEmailContent RenderPasswordRecovery(
            string username,
            string code,
            string actionUrl,
            DateTime expiresUtc,
            string? culture)
        {
            var language = ResolveLanguage(culture);
            var subject = language == AccountConstants.Cultures.Spanish
                ? "Recupera tu contraseña de command-q"
                : "Recover your command-q password";
            var values = CreateValues(username, code, actionUrl, expiresUtc, language);

            return Render("password-recovery", subject, language, values);
        }

        private static AccountEmailContent Render(
            string template,
            string subject,
            string language,
            IReadOnlyDictionary<string, string> values)
        {
            var htmlLayout = ReadTemplate("layout.html");
            var textLayout = ReadTemplate("layout.txt");
            var htmlContent = ReplaceValues(ReadTemplate($"{language}.{template}.html"), values, htmlEncode: true);
            var textContent = ReplaceValues(ReadTemplate($"{language}.{template}.txt"), values, htmlEncode: false);
            var layoutValues = new Dictionary<string, string>(values, StringComparer.Ordinal)
            {
                ["language"] = language,
                ["preheader"] = subject,
                ["footer"] = language == AccountConstants.Cultures.Spanish
                    ? "Este mensaje de seguridad fue enviado por command-q. Si no hiciste esta solicitud, puedes ignorarlo."
                    : "This security message was sent by command-q. If you did not make this request, you can safely ignore it."
            };

            var html = ReplaceValues(htmlLayout, layoutValues, htmlEncode: true)
                .Replace(ContentPlaceholder, htmlContent, StringComparison.Ordinal);
            var text = ReplaceValues(textLayout, layoutValues, htmlEncode: false)
                .Replace(ContentPlaceholder, textContent, StringComparison.Ordinal);

            return new AccountEmailContent(subject, html, text);
        }

        private static Dictionary<string, string> CreateValues(
            string username,
            string code,
            string actionUrl,
            DateTime expiresUtc,
            string language)
        {
            var culture = CultureInfo.GetCultureInfo(language);

            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["username"] = username,
                ["code"] = code,
                ["actionUrl"] = actionUrl,
                ["expiresUtc"] = expiresUtc.ToString("f", culture) + " UTC"
            };
        }

        private static string ReplaceValues(
            string template,
            IReadOnlyDictionary<string, string> values,
            bool htmlEncode)
        {
            foreach (var value in values)
            {
                var replacement = htmlEncode ? WebUtility.HtmlEncode(value.Value) : value.Value;
                template = template.Replace($"{{{{{value.Key}}}}}", replacement, StringComparison.Ordinal);
            }

            return template;
        }

        private static string ResolveLanguage(string? culture)
        {
            return culture?.StartsWith("es", StringComparison.OrdinalIgnoreCase) == true
                ? AccountConstants.Cultures.Spanish
                : AccountConstants.Cultures.English;
        }

        private static string ReadTemplate(string name)
        {
            var resourceName = $"{ResourcePrefix}.{name}";
            using var stream = Assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded account email template '{resourceName}' was not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
