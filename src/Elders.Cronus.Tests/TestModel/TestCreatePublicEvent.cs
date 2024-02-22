using System;

namespace Elders.Cronus.Tests.TestModel;

public class TestCreatePublicEvent : IPublicEvent
{
    public TestCreatePublicEvent(TestAggregateId id)
    {
        Id = id;
        Tenant = Id.Tenant;
    }

    public TestAggregateId Id { get; set; }

    public string Tenant { get; set; }

    public DateTimeOffset Timestamp => DateTimeOffset.UtcNow;
}

public class TestCreateEntityPublicEvent : IPublicEvent
{
    public TestCreateEntityPublicEvent(TestEntityId entityId)
    {
        this.EntityId = entityId;
        Tenant = entityId.NSS;
    }

    public TestEntityId EntityId { get; set; }

    public string Tenant { get; set; }

    public DateTimeOffset Timestamp => DateTimeOffset.UtcNow;
}
