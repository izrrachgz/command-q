namespace Api.Modules.Identity
{
    internal static class IdentityRoute
    {
        public const string AdminRoot = "admin";
        public const string AccountRoot = "accounts";
        public const string Accounts = "accounts";
        public const string Roles = "roles";
        public const string Me = "me";
        public const string Activity = Me + "/activity";
        public const string Preferences = Me + "/preferences";
        public const string AccountId = "{accountId:guid}";
        public const string AdminAccounts = AdminRoot + "/" + Accounts;
        public const string AdminRoles = AdminRoot + "/" + Roles;
        public const string AccountRoles = AccountId + "/" + Roles;
        public const string AccountUnlock = AccountId + "/unlock";
    }
}
