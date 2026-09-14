namespace IdentityServer.Modules.Accounts.Services.Email
{
    /// <summary>Owns the stable Data Protection purpose used for staged account-email payloads.</summary>
    internal static class AccountEmailDispatchProtection
    {
        /// <summary>The purpose shared by account-email payload protection and unprotection.</summary>
        public const string Purpose = "IdentityServer.AccountEmail.Dispatch.v1";
    }
}
