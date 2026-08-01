namespace Api.Modules.Contacts
{
    public static class ContactsApiModuleExtensions
    {
        public static ControllerActionEndpointConventionBuilder MapContactsModule(this ControllerActionEndpointConventionBuilder controllers)
        {
            return controllers;
        }
    }
}
