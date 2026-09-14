namespace IdentityServer.Modules.Accounts.Configuration
{
    /// <summary>Configures account confirmation, password recovery, email relay, retry, and retention behavior.</summary>
    internal sealed class AccountEmailOptions
    {
        /// <summary>Gets or sets the public frontend origin used to construct confirmation and recovery links.</summary>
        public string ApplicationBaseUrl { get; set; } = string.Empty;

        /// <summary>Gets or sets the Email API delivery profile requested for account messages.</summary>
        public string ProfileName { get; set; } = string.Empty;

        /// <summary>Gets or sets the email-confirmation challenge lifetime in minutes.</summary>
        public int ConfirmationLifetimeMinutes { get; set; }

        /// <summary>Gets or sets the password-recovery challenge lifetime in minutes.</summary>
        public int PasswordRecoveryLifetimeMinutes { get; set; }

        /// <summary>Gets or sets the verified password-reset grant lifetime in minutes.</summary>
        public int ResetGrantLifetimeMinutes { get; set; }

        /// <summary>Gets or sets the maximum failed link-and-code verification attempts.</summary>
        public int VerificationMaxFailedAttempts { get; set; }

        /// <summary>Gets or sets the maximum failed new-password submissions before administrator-required lockout.</summary>
        public int PasswordResetMaxFailedAttempts { get; set; }

        /// <summary>Gets or sets the minimum number of seconds between replacement email requests.</summary>
        public int ResendCooldownSeconds { get; set; }

        /// <summary>Gets or sets the maximum account-email deliveries allowed per account per day.</summary>
        public int DailyDeliveryLimit { get; set; }

        /// <summary>Gets or sets how many seconds the maintenance worker waits between polling cycles.</summary>
        public int DispatchPollSeconds { get; set; }

        /// <summary>Gets or sets the maximum number of due dispatches processed by one polling cycle.</summary>
        public int DispatchBatchSize { get; set; }

        /// <summary>Gets or sets the initial email-queue retry delay in seconds.</summary>
        public int DispatchRetryBaseSeconds { get; set; }

        /// <summary>Gets or sets the maximum email-queue retry delay in minutes.</summary>
        public int DispatchRetryMaxMinutes { get; set; }

        /// <summary>Gets or sets how many minutes elapse between retention cleanup operations.</summary>
        public int CleanupIntervalMinutes { get; set; }

        /// <summary>Gets or sets how many days consumed or expired account challenges are retained.</summary>
        public int ChallengeRetentionDays { get; set; }

        /// <summary>Gets or sets how many days identity security events are retained.</summary>
        public int SecurityEventRetentionDays { get; set; }
    }
}
