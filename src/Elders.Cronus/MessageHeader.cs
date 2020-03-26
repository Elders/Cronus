namespace Elders.Cronus
{
    public static class MessageHeader
    {
        public const string AggregateRootId = "ar_id";

        public const string AggregateRootRevision = "ar_revision";

        public const string AggregateRootEventPosition = "event_position";

        public const string CorelationId = "corelationid";

        public const string CausationId = "causationid";

        public const string MessageId = "messageid";

        public const string Tenant = "tenant";

        public const string BoundedContext = "bounded_context";

        public const string PublishTimestamp = "publish_timestamp";
    }
}
