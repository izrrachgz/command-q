namespace Api.Modules.Health
{
    public static class HealthApiModuleExtensions
    {
        public static ControllerActionEndpointConventionBuilder MapHealthModule(this ControllerActionEndpointConventionBuilder controllers)
        {
            return controllers;
        }
    }
}
