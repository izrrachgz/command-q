namespace Application.Modules.Notifications.Services
{
    /// <summary>
    /// Controls which logical SMTP profiles application notification workflows may select.
    /// </summary>
    public interface IEmailProfileSelectionPolicy
    {
        /// <summary>
        /// Validates a profile selected by an application workflow.
        /// </summary>
        /// <param name="profileName">The requested logical profile name, or <see langword="null"/> for the default.</param>
        void Validate(string? profileName);
    }
}
