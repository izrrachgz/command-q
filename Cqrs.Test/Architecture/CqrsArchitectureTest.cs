using Cqrs.Abstractions;

namespace Cqrs.Test.Architecture
{
    public sealed class CqrsArchitectureTest
    {
        [Fact]
        public void Cqrs_Should_Keep_A_Minimal_Dependency_Boundary()
        {
            string[] allowedPrefixes = ["Cqrs", "Microsoft.Extensions.DependencyInjection", "System"];
            string[] references = typeof(IRequestDispatcher).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .Where(name => !allowedPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
                .ToArray();

            Assert.Empty(references);
        }
    }
}
