using System;
using System.Collections.Generic;
using Machine.Specifications;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Tests.TestModel;
using Elders.Cronus.EventStore;

namespace Elders.Cronus.Tests
{
    [Subject("AggregateRoot")]
    public class When_build_aggregate_root_from_events
    {
        Establish context = () =>
        {
            id = new TestAggregateId();

            var commits = new List<AggregateCommit>();
            commits.Add(new AggregateCommit(id, 1, new List<IEvent>() { new TestCreateEvent(id) }));
            commits.Add(new AggregateCommit(id, 2, new List<IEvent>() { new TestUpdateEvent(id, "When_build_aggregate_root_from_events") }));

            eventStream = new EventStream(commits);
        };

        Because of = () => ar = eventStream.RestoreFromHistory<TestAggregateRoot>();

        It should_instansiate_aggregate_root = () => ar.ShouldNotBeNull();
        It should_instansiate_aggregate_root_with_valid_state = () => ar.State.Id.ShouldEqual(id);

        static TestAggregateId id;
        static EventStream eventStream;
        static TestAggregateRoot ar;
    }

    [Subject("AggregateRoot")]
    public class When_build_aggregate_root_from_history_without_the_initial_event
    {
        Establish context = () =>
        {
            id = new TestAggregateId();
            var commits = new List<AggregateCommit>();
            commits.Add(new AggregateCommit(id, 2, new List<IEvent>() { new TestUpdateEvent(id, "When_build_aggregate_root_from_history_without_the_initial_event") }));
            eventStream = new EventStream(commits);
        };

        Because of = () => expectedException = Catch.Exception(() => eventStream.RestoreFromHistory<TestAggregateRoot>());

        It an__AggregateRootException__should_be_thrown = () => expectedException.ShouldBeOfExactType<AggregateRootException>();

        static TestAggregateId id;
        static EventStream eventStream;
        static Exception expectedException;
    }


}
