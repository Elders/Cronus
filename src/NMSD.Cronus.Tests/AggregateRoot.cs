using System;
using System.Collections.Generic;
using Machine.Specifications;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Sample.Collaboration.Users;
using NMSD.Cronus.Sample.Collaboration.Users.Events;

namespace NMSD.Cronus.Tests
{
    [Subject("AggregateRoot")]
    public class When_build_aggregate_root_from_events
    {
        Establish context = () =>
        {
            id = new UserId(Guid.NewGuid());
            events = new List<IEvent>();
            events.Add(new UserCreated(id, "collaborator@cronus.com"));
            events.Add(new UserRenamed(id, "first", "last"));
        };

        Because of = () => ar = AggregateRootFactory.Build<User>(events);

        It should_instansiate_aggregate_root = () => ar.ShouldNotBeNull();
        It should_instansiate_aggregate_root_with_valid_state = () => ((IAggregateRootStateManager)ar).State.Id.ShouldEqual(id);

        static User ar;
        static UserId id;
        static List<IEvent> events;
    }

    [Subject("AggregateRoot")]
    public class When_build_aggregate_root_from_history_without_the_initial_event
    {
        Establish context = () =>
        {
            id = new UserId(Guid.NewGuid());
            events = new List<IEvent>();
            events.Add(new UserRenamed(id, "first", "last"));
        };

        Because of = () => expectedException = Catch.Exception(() => AggregateRootFactory.Build<User>(events));

        It an__AggregateRootException__should_be_thrown = () => expectedException.ShouldBeOfType<AggregateRootException>();

        static UserId id;
        static List<IEvent> events;
        static Exception expectedException;
    }
}
