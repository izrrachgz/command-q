namespace Cqrs.DependencyInjection
{
    internal sealed record RequestRegistration(Type RequestType, Type ResponseType, Type HandlerType);
}
