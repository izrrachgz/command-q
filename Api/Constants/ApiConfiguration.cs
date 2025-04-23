namespace Api.Constants
{
    public static class ApiConfiguration
    {
        #region Api

        public const string ApiKeyHeaderKeyName = @"ApiAccessKeys:Key";

        public const string ApiKeyHeaderKeyValue = @"ApiAccessKeys:Value";

        #endregion

        #region JWT

        public const string JwtSecretKey = @"Jwt:SecretKey";

        public const string JwtIssuer = @"Jwt:Issuer";

        public const string JwtAudience = @"Jwt:Audience";

        public const string JwtMinutesToExpire = @"Jwt:MinutesToExpire";

        #endregion
    }
}
