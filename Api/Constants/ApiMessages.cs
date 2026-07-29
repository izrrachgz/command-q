namespace Api.Constants
{
    internal static class ApiMessages
    {
        public const string NotValidApiClient = @"This API client is not valid to work with or client headers are not present in the request.";

        public const string NotValidJwtSecretKey = @"The JWT secret key is not valid to work with or is not present in the configuration file.";
    }
}
