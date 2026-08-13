namespace Email.Worker.Modules.Health
{
    /// <summary>
    /// Defines HTTP routes for email worker health endpoints.
    /// </summary>
    internal static class EmailWorkerHealthRoutes
    {
        public const string Live = "/health/live";
        public const string Ready = "/health/ready";
        public const string Checks = "/health/checks";
    }
}
