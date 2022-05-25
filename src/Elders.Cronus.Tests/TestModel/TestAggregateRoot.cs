using System;

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

        public TestEntity CreateEntity(TestEntityId id)
        {
            var evnt = new TestCreateEntityEvent(id);
            Apply(evnt);

            var entity = new TestEntity(this, id);
            return entity;
        }

        public void MakePublicEvent(TestEntityId id)
        {
            throw new NotImplementedException();
        }

        public void DoSomething(string text)
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

        public void MakeEntityEvent()
        {
            throw new NotImplementedException();
        }

        public void MakeEntityPublicEvent()
        {
            var @event = new TestCreateEntityPublicEvent(state.EntityId);
            Apply(@event);
        }

        public void MakeEntityEventAndPublicEvent()
        {
            throw new NotImplementedException();
        }
    }

    public class TestEntityState : EntityState<TestEntityId>
    {
        public override TestEntityId EntityId { get; set; }
    }
}
