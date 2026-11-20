using Messaging.Abstractions;

namespace Messaging.Test.Architecture
{
    public sealed class MessagingArchitectureTest
    {
        [Fact]
        public void Messaging_Should_Keep_A_Transport_Neutral_Dependency_Boundary()
        {
            string[] allowedPrefixes = ["Messaging", "System"];
            string[] references = typeof(IMessageSender).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .Where(name => !allowedPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
                .ToArray();

            Assert.Empty(references);
        }
    }
}
