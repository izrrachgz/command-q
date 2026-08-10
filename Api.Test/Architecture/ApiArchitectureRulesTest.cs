using Api.Modules;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Api.Test.Architecture
{
    public sealed class ApiArchitectureRulesTest
    {
        private static readonly Assembly ApiAssembly = typeof(ApiModuleExtensions).Assembly;

        [Fact]
        public void Controllers_Should_Live_In_Api_Module_Namespaces()
        {
            var violations = ApiAssembly.GetTypes()
                .Where(type => !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
                .Where(type => type.Namespace is null || !type.Namespace.StartsWith("Api.Modules.", StringComparison.Ordinal))
                .Select(type => type.FullName)
                .Order()
                .ToArray();

            Assert.Empty(violations);
        }

        [Fact]
        public void Module_Extensions_Should_Live_In_Api_Module_Namespaces()
        {
            var violations = ApiAssembly.GetTypes()
                .Where(type => type.Name.EndsWith("ApiModuleExtensions", StringComparison.Ordinal))
                .Where(type => type.Namespace is null || !type.Namespace.StartsWith("Api.Modules", StringComparison.Ordinal))
                .Select(type => type.FullName)
                .Order()
                .ToArray();

            Assert.Empty(violations);
        }

        [Fact]
        public void Api_Should_Not_Reference_Frontend_Assemblies()
        {
            var referencedAssemblies = ApiAssembly.GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .Where(name => name is not null)
                .ToHashSet();

            Assert.DoesNotContain("FrontApplication", referencedAssemblies);
            Assert.DoesNotContain("IdentityServer", referencedAssemblies);
        }

        [Fact]
        public void Api_Should_Compose_Email_Outbox_Without_Owning_Email_Delivery()
        {
            var referencedAssemblies = ApiAssembly.GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .Where(name => name is not null)
                .ToHashSet();

            Assert.Contains("Email.Outbox", referencedAssemblies);
            Assert.Contains("Email.Outbox.Persistence", referencedAssemblies);
            Assert.DoesNotContain("Email.Worker", referencedAssemblies);
        }
    }
}
