namespace Api.Constants
{
    internal static partial class ApiConfigKeys
    {
        public static class IdentityServerAdmin
        {
            public const string Section = "IdentityServerAdmin";
            public const string ApiKey = Section + ":ApiKey";
            public const string ApiKeyHeaderName = Section + ":ApiKeyHeaderName";
            public const string BaseUrl = Section + ":BaseUrl";
            public const string Enabled = Section + ":Enabled";
            public const string RolesPath = Section + ":RolesPath";
            public const string TimeoutSeconds = Section + ":TimeoutSeconds";
            public const string UsersPath = Section + ":UsersPath";
        }
    }
}
