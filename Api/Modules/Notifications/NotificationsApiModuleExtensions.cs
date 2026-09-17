namespace Api.Modules.Notifications;

/// <summary>Provides endpoint composition for the Notifications API module.</summary>
public static class NotificationsApiModuleExtensions
{
    /// <summary>Applies Notifications module conventions to the shared controller endpoint builder.</summary>
    /// <param name="controllers">The shared controller endpoint convention builder.</param>
    /// <returns>The supplied controller endpoint convention builder.</returns>
    public static ControllerActionEndpointConventionBuilder MapNotificationsModule(
        this ControllerActionEndpointConventionBuilder controllers)
    {
        return controllers;
    }
}
