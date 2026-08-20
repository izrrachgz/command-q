namespace Api.Modules.Configuration
{
    public static class ConfigurationApiModuleExtensions
    {
        public static ControllerActionEndpointConventionBuilder MapConfigurationModule(
            this ControllerActionEndpointConventionBuilder controllers)
        {
            return controllers;
        }
    }
}
