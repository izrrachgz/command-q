using System.Reflection;
using Persistence.Abstractions;
using Application.Migrations.Logging;

namespace Application.Migrations.Test.Architecture
{
    public sealed class ApplicationMigrationsArchitectureTest
    {
        private static readonly Assembly MigrationsAssembly = typeof(ApplicationMigrationAssembly).Assembly;
        private static readonly Assembly PersistenceAssembly = typeof(IWritableRepository).Assembly;

        [Fact]
        public void ApplicationMigrations_Should_Reference_Persistence_But_Not_Business_Or_Host_Projects()
        {
            var references = MigrationsAssembly.GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => name is not null)
                .Cast<string>()
                .ToHashSet(StringComparer.Ordinal);

            Assert.Contains("Persistence", references);
            Assert.DoesNotContain("Application", references);
            Assert.DoesNotContain("Domain", references);
            Assert.DoesNotContain("Api", references);
        }

        [Fact]
        public void Persistence_Should_Not_Reference_ApplicationMigrations()
        {
            var references = PersistenceAssembly.GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => name is not null)
                .Cast<string>()
                .ToHashSet(StringComparer.Ordinal);

            Assert.DoesNotContain("Application.Migrations", references);
        }

        [Fact]
        public void Migration_Log_EventIds_Should_Be_Unique_And_In_The_Project_Range()
        {
            var eventIds = typeof(ApplicationMigrationsEventIds)
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(field => field.IsLiteral && field.FieldType == typeof(int))
                .Select(field => (int)field.GetRawConstantValue()!)
                .ToArray();

            Assert.NotEmpty(eventIds);
            Assert.Equal(eventIds.Length, eventIds.Distinct().Count());
            Assert.All(
                eventIds,
                eventId => Assert.InRange(
                    eventId,
                    ApplicationMigrationsEventIdRanges.General,
                    ApplicationMigrationsEventIdRanges.General + 999));
        }
    }
}
