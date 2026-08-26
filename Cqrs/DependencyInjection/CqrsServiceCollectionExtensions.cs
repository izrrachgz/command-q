using Cqrs.Abstractions;
using Cqrs.Dispatching;
using Cqrs.Exceptions;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cqrs.DependencyInjection
{
    /// <summary>
    /// Registers the dispatcher and explicit, strongly typed request pipelines.
    /// </summary>
    public static class CqrsServiceCollectionExtensions
    {
        public static IServiceCollection AddCqrs(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddScoped<IRequestDispatcher, RequestDispatcher>();
            return services;
        }

        public static IServiceCollection AddRequestHandler<
            TRequest,
            TResponse,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(this IServiceCollection services)
            where TRequest : class, IRequest<TResponse>
            where THandler : class, IRequestHandler<TRequest, TResponse>
        {
            ArgumentNullException.ThrowIfNull(services);

            if (services.Any(descriptor =>
                    descriptor.ServiceType == typeof(RequestRegistration) &&
                    descriptor.ImplementationInstance is RequestRegistration registration &&
                    registration.RequestType == typeof(TRequest)))
            {
                throw new DuplicateRequestHandlerException(typeof(TRequest));
            }

            services.AddCqrs();
            services.AddSingleton(new RequestRegistration(typeof(TRequest), typeof(TResponse), typeof(THandler)));
            services.AddTransient<IRequestHandler<TRequest, TResponse>, THandler>();
            services.AddKeyedTransient<IRequestInvoker<TResponse>, RequestInvoker<TRequest, TResponse>>(typeof(TRequest));
            return services;
        }

        public static IServiceCollection AddPipelineBehavior<
            TRequest,
            TResponse,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TBehavior>(this IServiceCollection services)
            where TRequest : class, IRequest<TResponse>
            where TBehavior : class, IPipelineBehavior<TRequest, TResponse>
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddTransient<IPipelineBehavior<TRequest, TResponse>, TBehavior>();
            return services;
        }
    }
}
