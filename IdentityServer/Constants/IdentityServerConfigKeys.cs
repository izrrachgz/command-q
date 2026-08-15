namespace IdentityServer.Constants
{
    /// <summary>
    /// Provides configuration paths for the IdentityServer host.
    /// </summary>
    internal static partial class IdentityServerConfigKeys
    {
        public const string Section = "IdentityServer";

        public static string FullPath(string childPath)
        {
            return $"{Section}:{childPath}";
        }
    }
}
