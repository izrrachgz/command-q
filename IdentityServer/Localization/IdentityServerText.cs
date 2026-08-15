using System.Globalization;

namespace IdentityServer.Localization
{
    internal static class IdentityServerText
    {
        public static string Format(string message, params object[] args)
        {
            return args.Length > 0
                ? string.Format(CultureInfo.CurrentCulture, message, args)
                : message;
        }
    }
}
