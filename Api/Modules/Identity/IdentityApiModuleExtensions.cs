namespace Api.Modules.Identity
{
    public static class IdentityApiModuleExtensions
    {
        public static ControllerActionEndpointConventionBuilder MapIdentityModule(this ControllerActionEndpointConventionBuilder controllers)
        {
            return controllers;
        }
    }
}
