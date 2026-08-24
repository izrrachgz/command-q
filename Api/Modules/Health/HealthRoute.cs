namespace Api.Modules.Health
{
    internal static class HealthRoute
    {
        public const string Root = "/health";
        public const string Live = Root + "/live";
        public const string Ready = Root + "/ready";
        public const string Checks = Root + "/checks";
    }
}
