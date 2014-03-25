using System;
using System.Collections.Generic;
using Machine.Specifications;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Tests.TestModel;

namespace Elders.Cronus.Tests
{
    [Subject("AggregateRoot")]
    public class When_build_aggregate_root_from_events
    {
        Establish context = () =>
        {
            id = new TestAggregateId();

            events = new List<IEvent>();
            events.Add(new TestCreateEvent(id));
            events.Add(new TestUpdateEvent(id, "When_build_aggregate_root_from_events"));
        };

        Because of = () => ar = AggregateRootFactory.Build<TestAggregateRoot>(events);

        It should_instansiate_aggregate_root = () => ar.ShouldNotBeNull();
        It should_instansiate_aggregate_root_with_valid_state = () => ((IAggregateRootStateManager)ar).State.Id.ShouldEqual(id);

        static TestAggregateId id;
        static List<IEvent> events;
        static TestAggregateRoot ar;
    }

    [Subject("AggregateRoot")]
    public class When_build_aggregate_root_from_history_without_the_initial_event
    {
        Establish context = () =>
        {
            id = new TestAggregateId();
            events = new List<IEvent>();
            events.Add(new TestUpdateEvent(id, "When_build_aggregate_root_from_history_without_the_initial_event"));
        };

        Because of = () => expectedException = Catch.Exception(() => AggregateRootFactory.Build<TestAggregateRoot>(events));

        It an__AggregateRootException__should_be_thrown = () => expectedException.ShouldBeOfExactType<AggregateRootException>();

        static TestAggregateId id;
        static List<IEvent> events;
        static Exception expectedException;
    }

    
}
