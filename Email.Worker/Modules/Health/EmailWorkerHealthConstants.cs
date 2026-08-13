namespace Email.Worker.Modules.Health
{
    /// <summary>
    /// Defines health-check names, tags and messages for the email worker host.
    /// </summary>
    internal static class EmailWorkerHealthConstants
    {
        public const string SelfName = "email-worker";
        public const string OutboxPersistenceName = "email-outbox-persistence";
        public const string LiveTag = "live";
        public const string ReadyTag = "ready";
        public const string RunningMessage = "Email worker host is running.";
    }
}
