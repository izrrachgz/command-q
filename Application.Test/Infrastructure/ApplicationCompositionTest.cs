using Cqrs.Abstractions;
using FluentValidation;
using Application.Abstractions;
using Persistence.Abstractions;
using Localization.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Application.Modules.Identity.AccountActivity.Services;
using Application.Modules.Identity.Accounts.Commands;

namespace Application.Test.Infrastructure
{
    public sealed class ApplicationCompositionTest(IServiceProvider serviceProvider)
    {
        [Fact]
        public void Application_Composition_Should_Resolve_Core_Services()
        {
            _ = serviceProvider.GetRequiredService<IRequestDispatcher>();
            _ = serviceProvider.GetRequiredService<ITextProvider>();
            _ = serviceProvider.GetRequiredService<IReadOnlyRepository>();
            _ = serviceProvider.GetRequiredService<IWritableRepository>();
            _ = serviceProvider.GetRequiredService<IAccountActivityRecorder>();
        }

        [Fact]
        public void Application_Composition_Should_Resolve_Module_Implementations()
        {
            var applicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;
            var serviceTypes = applicationAssembly.GetTypes()
                .Where(type => !type.IsAbstract)
                .SelectMany(type => type.GetInterfaces())
                .Where(IsModuleImplementationContract)
                .Distinct()
                .OrderBy(type => type.FullName)
                .ToArray();

            foreach (var serviceType in serviceTypes)
                Assert.NotNull(serviceProvider.GetRequiredService(serviceType));
        }

        [Fact]
        public async Task Application_Composition_Should_Run_Validation_Before_The_Handler()
        {
            var dispatcher = serviceProvider.GetRequiredService<IRequestDispatcher>();
            var command = new CreateAccountCommand
            {
                Email = string.Empty,
                Username = string.Empty,
                IdentityIssuer = string.Empty,
                IdentitySubject = string.Empty
            };

            await Assert.ThrowsAsync<ValidationException>(
                () => dispatcher.Send(command, TestContext.Current.CancellationToken));
        }

        private static bool IsModuleImplementationContract(Type serviceType)
        {
            return serviceType.IsGenericType &&
                serviceType.GetGenericTypeDefinition() is var definition &&
                definition is not null &&
                (definition == typeof(IRequestHandler<,>) ||
                 definition == typeof(IValidator<>) ||
                 definition == typeof(IMapper<,>));
        }
    }
}
