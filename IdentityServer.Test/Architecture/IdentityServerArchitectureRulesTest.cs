using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using IdentityServer.Persistence;

namespace IdentityServer.Test.Architecture
{
    public sealed class IdentityServerArchitectureRulesTest
    {
        private static readonly Assembly IdentityServerAssembly = typeof(IdentityServerDbContext).Assembly;

        [Fact]
        public void IdentityServer_Should_Not_Reference_Application_Api_Or_Domain()
        {
            var references = IdentityServerAssembly.GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToHashSet();

            Assert.DoesNotContain("Api", references);
            Assert.DoesNotContain("Application", references);
            Assert.DoesNotContain("Domain", references);
        }

        [Fact]
        public void Controllers_Should_Be_Public_And_Live_In_Module_Namespaces()
        {
            var violations = IdentityServerAssembly.GetTypes()
                .Where(type => !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
                .Where(type => !type.IsPublic || type.Namespace?.StartsWith("IdentityServer.Modules.", StringComparison.Ordinal) != true)
                .Select(type => type.FullName)
                .Order()
                .ToArray();

            Assert.Empty(violations);
        }
    }
}
