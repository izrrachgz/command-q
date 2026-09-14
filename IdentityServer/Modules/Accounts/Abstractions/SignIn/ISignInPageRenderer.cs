namespace IdentityServer.Modules.Accounts.Abstractions.SignIn
{
    /// <summary>Renders the IdentityServer-hosted local sign-in page from its embedded template.</summary>
    public interface ISignInPageRenderer
    {
        /// <summary>Renders a sign-in page that preserves the validated authorization return URL.</summary>
        /// <param name="returnUrl">The local authorization URL to resume after successful authentication.</param>
        /// <param name="failed">Whether to display an invalid-credentials message.</param>
        /// <param name="lockedOut">Whether to display an account-lockout message.</param>
        /// <returns>The complete HTML document.</returns>
        string Render(string returnUrl, bool failed = false, bool lockedOut = false);
    }
}
