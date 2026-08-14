namespace Api.Constants
{
    internal static partial class ApiConfigKeys
    {
        public static class EmailOutbox
        {
            public const string Section = "EmailOutbox";
            public const string PersistenceSection = "Persistence";
            public const string Engine = "EmailOutbox:Persistence:Engine";
        }
    }
}
