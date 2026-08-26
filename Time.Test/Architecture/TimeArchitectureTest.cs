using System.Reflection;

namespace Time.Test.Architecture
{
    public sealed class TimeArchitectureTest
    {
        private static readonly string[] ForbiddenProjectDependencies =
        {
            "Api",
            "Application",
            "Application.Migrations",
            "Common",
            "Domain",
            "Email",
            "Email.Outbox",
            "Email.Outbox.Persistence",
            "Email.Worker",
            "IdentityServer",
            "Localization",
            "Persistence"
        };

        [Fact]
        public void Time_Should_Remain_Independent_From_Feature_And_Infrastructure_Projects()
        {
            Assembly assembly = typeof(UtcTime).Assembly;
            var references = assembly.GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => name is not null)
                .Cast<string>()
                .ToHashSet(StringComparer.Ordinal);

            Assert.DoesNotContain(ForbiddenProjectDependencies, references.Contains);
        }
    }
}
