namespace Email.Configuration
{
    /// <summary>
    /// Selects the SMTP authentication mechanism and its corresponding values.
    /// </summary>
    public enum EmailAuthenticationType
    {
        Anonymous = 0,
        Login = 1,
        Plain = 2,
        OAuth = 3
    }
}
