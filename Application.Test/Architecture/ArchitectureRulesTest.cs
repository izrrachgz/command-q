using Api.Modules;
using Common.Cqrs;
using Domain.Entities;
using System.Reflection;
using Persistence.Abstractions;

namespace Application.Test.Architecture
{
    public sealed class ArchitectureRulesTest
    {
        private static readonly Assembly ApiAssembly = typeof(ApiModuleExtensions).Assembly;
        private static readonly Assembly ApplicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;
        private static readonly Assembly DomainAssembly = typeof(BaseEntity).Assembly;
        private static readonly Assembly PersistenceAssembly = typeof(IReadOnlyRepository).Assembly;

        [Fact]
        public void Domain_Should_Not_Reference_Application_Persistence_Or_Api()
        {
            var referencedAssemblies = DomainAssembly.GetReferencedAssemblies().Select(assembly => assembly.Name).ToHashSet();

            Assert.DoesNotContain("Application", referencedAssemblies);
            Assert.DoesNotContain("Persistence", referencedAssemblies);
            Assert.DoesNotContain("Api", referencedAssemblies);
        }

        [Fact]
        public void Application_Account_Should_Not_Own_Authentication_Credentials_Or_Sessions()
        {
            var accountType = DomainAssembly.GetType("Domain.Modules.Identity.Account");

            Assert.NotNull(accountType);
            Assert.Null(accountType.GetProperty("Password"));
            Assert.DoesNotContain(
                DomainAssembly.GetTypes(),
                type => string.Equals(type.Name, "AccountSession", StringComparison.Ordinal));
            Assert.DoesNotContain(
                ApiAssembly.GetTypes(),
                type => type.Name is "JwtTokenService" or "RefreshTokenService" or "AuthService");
        }

        [Fact]
        public void Application_Should_Not_Reference_Api()
        {
            var referencedAssemblies = ApplicationAssembly.GetReferencedAssemblies().Select(assembly => assembly.Name).ToHashSet();

            Assert.DoesNotContain("Api", referencedAssemblies);
        }

        [Fact]
        public void Application_Notifications_Should_Use_Email_Contracts_Without_Owning_Delivery_Infrastructure()
        {
            var referencedAssemblies = ApplicationAssembly.GetReferencedAssemblies().Select(assembly => assembly.Name).ToHashSet();

            Assert.Contains("Email", referencedAssemblies);
            Assert.Contains("Email.Outbox", referencedAssemblies);
            Assert.DoesNotContain("Email.Outbox.Persistence", referencedAssemblies);
            Assert.DoesNotContain("Email.Worker", referencedAssemblies);
        }

        [Fact]
        public void Persistence_Should_Not_Reference_Application_Or_Api()
        {
            var referencedAssemblies = PersistenceAssembly.GetReferencedAssemblies().Select(assembly => assembly.Name).ToHashSet();

            Assert.DoesNotContain("Application", referencedAssemblies);
            Assert.DoesNotContain("Api", referencedAssemblies);
        }

        [Fact]
        public void Query_Handlers_Should_Not_Depend_On_Writable_Repository()
        {
            var violations = ApplicationAssembly.GetTypes()
                .Where(type => !type.IsAbstract && ImplementsOpenGeneric(type, typeof(IQueryHandler<,>)))
                .Where(type => ConstructorParameters(type).Any(parameter => parameter.ParameterType == typeof(IWritableRepository)))
                .Select(type => type.FullName)
                .Order()
                .ToArray();

            Assert.Empty(violations);
        }

        [Fact]
        public void Command_Handlers_Should_Not_Depend_On_ReadOnly_Repository()
        {
            var violations = ApplicationAssembly.GetTypes()
                .Where(type => !type.IsAbstract && (ImplementsOpenGeneric(type, typeof(ICommandHandler<,>)) || ImplementsOpenGeneric(type, typeof(ICommandHandler<>))))
                .Where(type => ConstructorParameters(type).Any(parameter => parameter.ParameterType == typeof(IReadOnlyRepository)))
                .Select(type => type.FullName)
                .Order()
                .ToArray();

            Assert.Empty(violations);
        }

        [Fact]
        public void Application_Module_Types_Should_Use_Module_Namespaces()
        {
            var violations = ApplicationAssembly.GetTypes()
                .Select(type => new { Type = type, type.Namespace })
                .Where(type => type.Namespace is not null)
                .Where(type => type.Namespace!.StartsWith("Application.Modules.", StringComparison.Ordinal))
                .Where(type => !IsAllowedApplicationModuleNamespace(type.Namespace!))
                .Select(type => type.Type.FullName)
                .Order()
                .ToArray();

            Assert.Empty(violations);
        }

        [Fact]
        public void Domain_Module_Types_Should_Use_Module_Namespaces()
        {
            var violations = DomainAssembly.GetTypes()
                .Select(type => new { Type = type, type.Namespace })
                .Where(type => type.Namespace is not null)
                .Where(type => type.Namespace!.StartsWith("Domain.Modules.", StringComparison.Ordinal))
                .Where(type => !IsAllowedDomainModuleNamespace(type.Namespace!))
                .Select(type => type.Type.FullName)
                .Order()
                .ToArray();

            Assert.Empty(violations);
        }

        [Fact]
        public void Api_Module_Types_Should_Use_Module_Namespaces()
        {
            var violations = ApiAssembly.GetTypes()
                .Select(type => new { Type = type, type.Namespace })
                .Where(type => type.Namespace is not null)
                .Where(type => type.Namespace!.StartsWith("Api.Modules.", StringComparison.Ordinal))
                .Where(type => !IsAllowedApiModuleNamespace(type.Namespace!))
                .Select(type => type.Type.FullName)
                .Order()
                .ToArray();

            Assert.Empty(violations);
        }

        [Fact]
        public void Logging_EventIds_Should_Not_Collide()
        {
            var duplicates = GetEventIds(ApiAssembly, ApplicationAssembly, CommonAssembly(), PersistenceAssembly)
                .GroupBy(eventId => eventId.Value)
                .Where(group => group.Count() > 1)
                .Select(group => $"{group.Key}: {string.Join(", ", group.Select(item => item.Name).Order())}")
                .Order()
                .ToArray();

            Assert.Empty(duplicates);
        }

        [Fact]
        public void Module_Logging_EventIds_Should_Use_Module_Ranges()
        {
            var violations = GetEventIds(ApiAssembly, ApplicationAssembly)
                .Where(eventId => eventId.DeclaringType.Namespace?.Contains(".Modules.", StringComparison.Ordinal) == true)
                .Where(eventId => !IsInExpectedModuleRange(eventId.DeclaringType.Namespace!, eventId.Value))
                .Select(eventId => $"{eventId.DeclaringType.FullName}.{eventId.Name}: {eventId.Value}")
                .Order()
                .ToArray();

            Assert.Empty(violations);
        }

        private static bool ImplementsOpenGeneric(Type type, Type openGenericType)
        {
            return type.GetInterfaces()
                .Any(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == openGenericType);
        }

        private static IEnumerable<ParameterInfo> ConstructorParameters(Type type)
        {
            return type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(constructor => constructor.GetParameters());
        }

        private static bool IsAllowedApplicationModuleNamespace(string @namespace)
        {
            return StartsWithAny(@namespace,
                "Application.Modules.Identity",
                "Application.Modules.Contacts",
                "Application.Modules.Addresses",
                "Application.Modules.Files",
                "Application.Modules.Health",
                "Application.Modules.Notifications");
        }

        private static bool IsAllowedDomainModuleNamespace(string @namespace)
        {
            return StartsWithAny(@namespace,
                "Domain.Modules.Identity",
                "Domain.Modules.Contacts",
                "Domain.Modules.Addresses",
                "Domain.Modules.Files",
                "Domain.Modules.Health",
                "Domain.Modules.Events",
                "Domain.Modules.Registers");
        }

        private static bool IsAllowedApiModuleNamespace(string @namespace)
        {
            return StartsWithAny(@namespace,
                "Api.Modules.Identity",
                "Api.Modules.Contacts",
                "Api.Modules.Addresses",
                "Api.Modules.Files",
                "Api.Modules.Health");
        }

        private static bool StartsWithAny(string value, params string[] prefixes)
        {
            return prefixes.Any(prefix => value.Equals(prefix, StringComparison.Ordinal) || value.StartsWith(prefix + ".", StringComparison.Ordinal));
        }

        private static Assembly CommonAssembly()
        {
            return typeof(Common.Abstractions.ITaskRunner).Assembly;
        }

        private static IEnumerable<EventIdValue> GetEventIds(params Assembly[] assemblies)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.Name.EndsWith("EventIds", StringComparison.Ordinal))
                .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(field => field.FieldType == typeof(int))
                    .Select(field => new EventIdValue(type, field.Name, (int)field.GetRawConstantValue()!)));
        }

        private static bool IsInExpectedModuleRange(string @namespace, int eventId)
        {
            return @namespace switch
            {
                var value when value.StartsWith("Api.Modules.Identity", StringComparison.Ordinal) => IsInRange(eventId, 101000),
                var value when value.StartsWith("Api.Modules.Health", StringComparison.Ordinal) => IsInRange(eventId, 102000),
                var value when value.StartsWith("Application.Modules.Health", StringComparison.Ordinal) => IsInRange(eventId, 201000),
                var value when value.StartsWith("Application.Modules.Identity", StringComparison.Ordinal) => IsInRange(eventId, 202000),
                var value when value.StartsWith("Application.Modules.Files", StringComparison.Ordinal) => IsInRange(eventId, 203000),
                var value when value.StartsWith("Application.Modules.Contacts", StringComparison.Ordinal) => IsInRange(eventId, 204000),
                var value when value.StartsWith("Application.Modules.Addresses", StringComparison.Ordinal) => IsInRange(eventId, 205000),
                var value when value.StartsWith("Application.Modules.Notifications", StringComparison.Ordinal) => IsInRange(eventId, 206000),
                _ => false
            };
        }

        private static bool IsInRange(int eventId, int rangeStart)
        {
            return eventId >= rangeStart && eventId < rangeStart + 1000;
        }

        private sealed record EventIdValue(Type DeclaringType, string Name, int Value);
    }
}
