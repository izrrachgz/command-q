namespace IdentityServer.Modules.Accounts
{
    public interface ISignInPageRenderer
    {
        string Render(string returnUrl, bool failed = false, bool lockedOut = false);
    }
}
