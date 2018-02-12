namespace Elders.Cronus.Tests.TestModel
{
    public class TestAggregateRoot : AggregateRoot<TestAggregateRootState>
    {
        TestAggregateRoot() { }
        public TestAggregateRoot(TestAggregateId id)
        {
            var @event = new TestCreateEvent(id);
            Apply(@event);
        }

        public void CreateEntity(TestEntityId id)
        {
            var evnt = new TestCreateEntityEvent(id);
            Apply(evnt);
        }

        public void Update(string text)
        {
            var @event = new TestUpdateEvent(state.Id, text);
            Apply(@event);
        }

        public TestAggregateRootState State { get { return base.state; } }
    }

    public class TestEntity : Entity<TestAggregateRoot, TestEntityState>
    {
        public TestEntity(TestAggregateRoot root, TestEntityId entityId)
            : base(root, entityId)
        {

        }
    }

    public class TestEntityState : EntityState<TestEntityId>
    {
        public override TestEntityId EntityId { get; set; }
    }

}
