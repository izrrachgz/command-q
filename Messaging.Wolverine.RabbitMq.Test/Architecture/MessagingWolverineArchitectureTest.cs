using Messaging.Wolverine.RabbitMq.DependencyInjection;

namespace Messaging.Wolverine.RabbitMq.Test.Architecture
{
    public sealed class MessagingWolverineArchitectureTest
    {
        [Fact]
        public void Adapter_Should_Not_Reference_Application_Or_Unrelated_CommandQ_Libraries()
        {
            string[] forbiddenReferences = ["Application", "Common", "Cqrs", "Domain", "Localization", "Persistence", "Time"];
            string[] references = typeof(RabbitMqMessagingExtensions).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            Assert.DoesNotContain(forbiddenReferences, references.Contains);
        }
    }
}
