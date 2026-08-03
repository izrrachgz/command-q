using Email.Logging;
using System.Reflection;
using Email.Abstractions;

namespace Email.Test.Architecture
{
    public sealed class EmailArchitectureTest
    {
        private static readonly string[] ForbiddenProjectDependencies =
        {
            "Api",
            "Application",
            "Domain",
            "IdentityServer",
            "Persistence"
        };

        [Fact]
        public void Email_Should_Not_Reference_Backend_Host_Or_Domain_Projects()
        {
            var references = typeof(IEmailSender).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => name is not null)
                .Cast<string>()
                .ToHashSet(StringComparer.Ordinal);

            Assert.DoesNotContain(ForbiddenProjectDependencies, references.Contains);
        }

        [Fact]
        public void Email_Log_Event_Ids_Should_Be_Unique_And_In_The_General_Range()
        {
            var eventIds = typeof(EmailEventIds)
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
                    EmailEventIdRanges.General,
                    EmailEventIdRanges.General + 999));
        }
    }
}
