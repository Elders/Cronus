namespace Elders.Cronus
{
    public static class MessageHeader
    {
        public const string AggregateRootId = "ar_id";

        public const string AggregateRootRevision = "ar_revision";

        public const string AggregateCommitTimestamp = "ar_commit_timestamp";

        public const string AggregateRootEventPosition = "event_position";

        public const string CorelationId = "corelationid";

        public const string CausationId = "causationid";

        public const string MessageId = "messageid";

        public const string MessageType = "messagetype";

        public const string Tenant = "tenant";

        public const string BoundedContext = "bounded_context";

        public const string PublishTimestamp = "publish_timestamp";

        public const string RecipientHandlers = "recipient_handlers";

        public const string RecipientBoundedContext = "recipient_bounded_context";

        public const string TTL = "ttl";
    }
}
