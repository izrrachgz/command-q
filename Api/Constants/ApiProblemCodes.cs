namespace Api.Constants
{
    /// <summary>
    /// Stable machine-readable codes exposed by API problem responses.
    /// </summary>
    internal static class ApiProblemCodes
    {
        public const string BadRequest = "request.bad_request";
        public const string ValidationFailed = "request.validation_failed";
        public const string Unauthorized = "authentication.required";
        public const string Forbidden = "authorization.forbidden";
        public const string ApiClientInvalid = "api_client.invalid";
        public const string NotFound = "resource.not_found";
        public const string Conflict = "resource.conflict";
        public const string InternalError = "server.internal_error";
        public const string ServiceUnavailable = "server.service_unavailable";
        public const string UpstreamFailure = "server.upstream_failure";

        public static class Accounts
        {
            public const string ProfileConflict = "accounts.profile_conflict";
            public const string ProfileMissing = "accounts.profile_missing";
            public const string PreferencesMissing = "accounts.preferences_missing";
            public const string PreferencesUpdateFailed = "accounts.preferences_update_failed";
            public const string RoleSelfRemoval = "accounts.role_self_removal";
            public const string RolesUpdateFailed = "accounts.roles_update_failed";
        }

        public static class Addresses
        {
            public const string CreationFailed = "addresses.creation_failed";
            public const string NotFound = "addresses.not_found";
        }

        public static class Contacts
        {
            public const string CreationFailed = "contacts.creation_failed";
            public const string NotFound = "contacts.not_found";
        }

        public static class Files
        {
            public const string Required = "files.required";
            public const string TooLarge = "files.too_large";
            public const string TypeNotAllowed = "files.type_not_allowed";
            public const string UploadFailed = "files.upload_failed";
        }
    }
}
