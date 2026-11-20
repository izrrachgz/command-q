using Messaging.Models;

namespace Messaging.Test
{
    public sealed class MessageContextTest
    {
        [Fact]
        public void Constructor_Should_Normalize_Sent_Time_To_Utc()
        {
            var sentAt = new DateTimeOffset(2026, 11, 16, 10, 30, 0, TimeSpan.FromHours(-7));

            var context = new MessageContext(Guid.NewGuid(), "correlation", "causation", sentAt, 2, new Dictionary<string, string>());

            Assert.Equal(TimeSpan.Zero, context.SentAtUtc.Offset);
            Assert.Equal(sentAt.UtcDateTime, context.SentAtUtc.UtcDateTime);
            Assert.Equal(2, context.DeliveryAttempt);
        }

        [Fact]
        public void Constructor_Should_Reject_Empty_Message_Id()
        {
            Assert.Throws<ArgumentException>(() =>
                new MessageContext(Guid.Empty, null, null, DateTimeOffset.UtcNow, 1, new Dictionary<string, string>()));
        }

        [Fact]
        public void Constructor_Should_Reject_Invalid_Delivery_Attempt()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new MessageContext(Guid.NewGuid(), null, null, DateTimeOffset.UtcNow, 0, new Dictionary<string, string>()));
        }
    }
}
