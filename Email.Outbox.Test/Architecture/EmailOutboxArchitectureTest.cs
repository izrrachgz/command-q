using Email.Worker;
using System.Reflection;
using Email.Outbox.Logging;
using Email.Worker.Logging;
using Persistence.Abstractions;
using Email.Outbox.Persistence;
using Email.Outbox.Abstractions;
using Email.Outbox.Persistence.Logging;

namespace Email.Outbox.Test.Architecture
{
    public sealed class EmailOutboxArchitectureTest
    {
        private static readonly Assembly OutboxAssembly = typeof(IEmailOutbox).Assembly;
        private static readonly Assembly OutboxPersistenceAssembly = typeof(EmailOutboxPersistenceAssembly).Assembly;
        private static readonly Assembly WorkerAssembly = typeof(EmailWorkerAssembly).Assembly;
        private static readonly Assembly PersistenceAssembly = typeof(IWritableRepository).Assembly;

        [Fact]
        public void Outbox_Should_Reference_Email_But_Not_Persistence_Or_Backend_Hosts()
        {
            var references = References(OutboxAssembly);

            Assert.Contains("Email", references);
            Assert.DoesNotContain("Persistence", references);
            Assert.DoesNotContain("Application", references);
            Assert.DoesNotContain("Api", references);
            Assert.DoesNotContain("IdentityServer", references);
        }

        [Fact]
        public void OutboxPersistence_Should_Reference_Outbox_And_Persistence_But_Not_Email_Or_Application()
        {
            var references = References(OutboxPersistenceAssembly);

            Assert.Contains("Email.Outbox", references);
            Assert.Contains("Persistence", references);
            Assert.DoesNotContain("Email", references);
            Assert.DoesNotContain("Application", references);
            Assert.DoesNotContain("Api", references);
        }

        [Fact]
        public void Persistence_Should_Not_Reference_Email_Outbox_Projects()
        {
            var references = References(PersistenceAssembly);

            Assert.DoesNotContain("Email.Outbox", references);
            Assert.DoesNotContain("Email.Outbox.Persistence", references);
        }

        [Fact]
        public void Worker_Should_Compose_Email_Outbox_And_Persistence()
        {
            var references = References(WorkerAssembly);

            Assert.Contains("Email", references);
            Assert.Contains("Email.Outbox", references);
            Assert.Contains("Email.Outbox.Persistence", references);
            Assert.DoesNotContain("Application", references);
            Assert.DoesNotContain("Api", references);
        }

        [Fact]
        public void EventIds_Should_Be_Unique_And_Within_Each_Project_Range()
        {
            AssertEventIds(typeof(EmailOutboxEventIds), EmailOutboxEventIdRanges.General);
            AssertEventIds(
                typeof(EmailOutboxPersistenceEventIds),
                EmailOutboxPersistenceEventIdRanges.General);
            AssertEventIds(typeof(EmailWorkerEventIds), EmailWorkerEventIdRanges.General);
        }

        private static HashSet<string> References(Assembly assembly)
        {
            return assembly.GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => name is not null)
                .Cast<string>()
                .ToHashSet(StringComparer.Ordinal);
        }

        private static void AssertEventIds(Type eventIdsType, int rangeStart)
        {
            var eventIds = eventIdsType
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(field => field.IsLiteral && field.FieldType == typeof(int))
                .Select(field => (int)field.GetRawConstantValue()!)
                .ToArray();

            Assert.NotEmpty(eventIds);
            Assert.Equal(eventIds.Length, eventIds.Distinct().Count());
            Assert.All(eventIds, eventId => Assert.InRange(eventId, rangeStart, rangeStart + 999));
        }
    }
}
