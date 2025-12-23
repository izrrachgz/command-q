namespace Localization.Abstractions
{
    public interface ITextProvider
    {
        string Get(string key, params object[] args);
    }
}
