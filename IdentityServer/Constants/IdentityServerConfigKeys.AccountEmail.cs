namespace IdentityServer.Constants
{
    internal static partial class IdentityServerConfigKeys
    {
        public static class AccountEmail
        {
            public const string Section = @"AccountEmail";
            public const string ApplicationBaseUrl = @"AccountEmail:ApplicationBaseUrl";
            public const string ConfirmationLifetimeMinutes = @"AccountEmail:ConfirmationLifetimeMinutes";
            public const string PasswordRecoveryLifetimeMinutes = @"AccountEmail:PasswordRecoveryLifetimeMinutes";
            public const string ResetGrantLifetimeMinutes = @"AccountEmail:ResetGrantLifetimeMinutes";
            public const string VerificationMaxFailedAttempts = @"AccountEmail:VerificationMaxFailedAttempts";
            public const string PasswordResetMaxFailedAttempts = @"AccountEmail:PasswordResetMaxFailedAttempts";
            public const string ResendCooldownSeconds = @"AccountEmail:ResendCooldownSeconds";
            public const string DailyDeliveryLimit = @"AccountEmail:DailyDeliveryLimit";
            public const string DispatchPollSeconds = @"AccountEmail:DispatchPollSeconds";
            public const string DispatchBatchSize = @"AccountEmail:DispatchBatchSize";
            public const string DispatchRetryBaseSeconds = @"AccountEmail:DispatchRetryBaseSeconds";
            public const string DispatchRetryMaxMinutes = @"AccountEmail:DispatchRetryMaxMinutes";
            public const string CleanupIntervalMinutes = @"AccountEmail:CleanupIntervalMinutes";
            public const string ChallengeRetentionDays = @"AccountEmail:ChallengeRetentionDays";
            public const string SecurityEventRetentionDays = @"AccountEmail:SecurityEventRetentionDays";
        }

    }
}
