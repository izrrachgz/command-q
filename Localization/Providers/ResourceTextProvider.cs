using Public;
using System.Globalization;
using Localization.Abstractions;

namespace Localization.Providers
{
    public class ResourceTextProvider : ITextProvider
    {
        public string Get(string key, params object[] args)
        {
            var value = Messages.ResourceManager
                .GetString(key, CultureInfo.CurrentUICulture);

            if (value == null) return key;

            return args.Length > 0
                ? string.Format(value, args)
                : value;
        }
    }
}
