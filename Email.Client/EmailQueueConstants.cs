namespace Email.Client;

public static class EmailQueueConstants
{
    public const string ConfigurationSection = "EmailApiClient";
    public const string DefaultProfileName = "default";
    public const int MaxIdempotencyKeyLength = 256;
    public const int MaxReferenceLength = 128;
    public const int MaxProfileNameLength = 64;
}
