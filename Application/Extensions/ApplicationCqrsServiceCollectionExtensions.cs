using Cqrs.Abstractions;
using Cqrs.DependencyInjection;
using Application.Validation;
using Application.RowLevelSecurity;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions
{
    internal static class ApplicationCqrsServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationRequest<
            TRequest,
            TResponse,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(this IServiceCollection services)
            where TRequest : class, IRequest<TResponse>
            where THandler : class, IRequestHandler<TRequest, TResponse>
        {
            services.AddRequestHandler<TRequest, TResponse, THandler>();
            services.AddPipelineBehavior<TRequest, TResponse, RequestValidationBehavior<TRequest, TResponse>>();
            services.AddPipelineBehavior<TRequest, TResponse, AccountRowLevelSecurityBehavior<TRequest, TResponse>>();
            return services;
        }
    }
}
