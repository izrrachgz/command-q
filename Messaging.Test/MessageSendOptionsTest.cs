using Messaging.Models;

namespace Messaging.Test
{
    public sealed class MessageSendOptionsTest
    {
        [Fact]
        public void Defaults_Should_Not_Require_Metadata()
        {
            var options = new MessageSendOptions();

            Assert.Empty(options.Headers);
            Assert.Null(options.DeliverByUtc);
            Assert.Null(options.CorrelationId);
            Assert.Null(options.CausationId);
            Assert.Null(options.DeduplicationId);
            Assert.Null(options.GroupId);
            Assert.Null(options.PartitionKey);
        }
    }
}
