using System.Net;
using System.Reflection;
using System.Text;
using IdentityServer.Localization;
using IdentityServer.Resources;

namespace IdentityServer.Modules.Accounts
{
    internal sealed class SignInPageRenderer : ISignInPageRenderer
    {
        private const string AlertMarkupToken = "{{AlertMarkup}}";
        private const string BrandMarkToken = "{{BrandMark}}";
        private const string BrandNameToken = "{{BrandName}}";
        private const string DocumentLanguageToken = "{{DocumentLanguage}}";
        private const string HeadingToken = "{{Heading}}";
        private const string IntroToken = "{{Intro}}";
        private const string PasswordLabelToken = "{{PasswordLabel}}";
        private const string RememberMeLabelToken = "{{RememberMeLabel}}";
        private const string SignInActionToken = "{{SignInAction}}";
        private const string SubmitButtonToken = "{{SubmitButton}}";
        private const string TemplateResourceName = "IdentityServer.Modules.Accounts.Templates.sign-in.html";
        private const string TitleToken = "{{Title}}";
        private const string UsernameOrEmailLabelToken = "{{UsernameOrEmailLabel}}";

        private static readonly Lazy<string> Template = new(LoadTemplate);

        public string Render(string returnUrl, bool failed = false, bool lockedOut = false)
        {
            return Template.Value
                .Replace(AlertMarkupToken, BuildAlertMarkup(failed, lockedOut), StringComparison.Ordinal)
                .Replace(BrandMarkToken, Encode(IdentityServerMessages.SignIn_BrandMark), StringComparison.Ordinal)
                .Replace(BrandNameToken, Encode(IdentityServerMessages.SignIn_BrandName), StringComparison.Ordinal)
                .Replace(DocumentLanguageToken, Encode(IdentityServerMessages.SignIn_DocumentLanguage), StringComparison.Ordinal)
                .Replace(HeadingToken, Encode(IdentityServerMessages.SignIn_Heading), StringComparison.Ordinal)
                .Replace(IntroToken, Encode(IdentityServerMessages.SignIn_Intro), StringComparison.Ordinal)
                .Replace(PasswordLabelToken, Encode(IdentityServerMessages.SignIn_PasswordLabel), StringComparison.Ordinal)
                .Replace(RememberMeLabelToken, Encode(IdentityServerMessages.SignIn_RememberMeLabel), StringComparison.Ordinal)
                .Replace(SignInActionToken, BuildSignInAction(returnUrl), StringComparison.Ordinal)
                .Replace(SubmitButtonToken, Encode(IdentityServerMessages.SignIn_SubmitButton), StringComparison.Ordinal)
                .Replace(TitleToken, Encode(IdentityServerMessages.SignIn_PageTitle), StringComparison.Ordinal)
                .Replace(UsernameOrEmailLabelToken, Encode(IdentityServerMessages.SignIn_UsernameOrEmailLabel), StringComparison.Ordinal);
        }

        private static string BuildAlertMarkup(bool failed, bool lockedOut)
        {
            if (!failed)
            {
                return string.Empty;
            }

            string error = lockedOut
                ? IdentityServerMessages.SignIn_LockedOut
                : IdentityServerMessages.SignIn_InvalidCredentials;

            return $"""<p class="alert">{WebUtility.HtmlEncode(error)}</p>""";
        }

        private static string BuildSignInAction(string returnUrl)
        {
            string action = $"{AccountConstants.Routes.SignInPath}?{AccountConstants.ReturnUrlParameter}={WebUtility.UrlEncode(returnUrl)}";
            return WebUtility.HtmlEncode(action);
        }

        private static string LoadTemplate()
        {
            var assembly = typeof(SignInPageRenderer).GetTypeInfo().Assembly;
            using var stream = assembly.GetManifestResourceStream(TemplateResourceName)
                ?? throw new InvalidOperationException(
                    IdentityServerText.Format(IdentityServerMessages.SignIn_TemplateMissing, TemplateResourceName));
            using var reader = new StreamReader(stream, Encoding.UTF8);

            return reader.ReadToEnd();
        }

        private static string Encode(string value)
        {
            return WebUtility.HtmlEncode(value);
        }
    }
}
