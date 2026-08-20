namespace Api.Modules.Identity
{
    internal static class IdentityRouteConstants
    {
        public const string AdminRoot = "Admin";
        public const string AccountRoot = "Account";
        public const string Accounts = "accounts";
        public const string Roles = "roles";
        public const string Create = "create";
        public const string Me = "me";
        public const string Activity = "activity";
        public const string Preferences = "preferences";
        public const string AccountId = "{accountId:guid}";
        public const string AdminAccounts = AdminRoot + "/" + Accounts;
        public const string AdminRoles = AdminRoot + "/" + Roles;
        public const string AccountRoles = AccountId + "/" + Roles;
    }
}
