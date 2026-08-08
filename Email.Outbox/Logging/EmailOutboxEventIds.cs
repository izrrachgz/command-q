namespace Email.Outbox.Logging
{
    internal static class EmailOutboxEventIds
    {
        public const int BatchClaimed = EmailOutboxEventIdRanges.General + 0;
        public const int PayloadInvalid = EmailOutboxEventIdRanges.General + 1;
        public const int BatchSent = EmailOutboxEventIdRanges.General + 2;
        public const int DeliveryFailed = EmailOutboxEventIdRanges.General + 3;
        public const int RecordsReleased = EmailOutboxEventIdRanges.General + 4;
    }
}
