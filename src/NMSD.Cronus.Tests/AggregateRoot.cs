using System;
using System.Collections.Generic;
using NMSD.Cronus.Core.Eventing;
using Machine.Specifications;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Core.DomainModelling;

namespace NMSD.Cronus.Tests
{
    [Subject("AggregateRoot")]
    public class When_build_aggregate_root_from_events
    {
        Establish context = () =>
        {
            id = new CollaboratorId(Guid.NewGuid());
            events = new List<IEvent>();
            events.Add(new NewCollaboratorCreated(id, "collaborator@cronus.com"));
            events.Add(new CollaboratorRenamed(id, "first", "last"));
        };

        Because of = () => ar = AggregateRootFactory.Build<Collaborator>(events);

        It should_instansiate_aggregate_root = () => ar.ShouldNotBeNull();
        It should_instansiate_aggregate_root_with_valid_state = () => ((IAggregateRootStateManager)ar).State.Id.ShouldEqual(id);

        static Collaborator ar;
        static CollaboratorId id;
        static List<IEvent> events;
    }

    [Subject("AggregateRoot")]
    public class When_build_aggregate_root_from_history_without_the_initial_event
    {
        Establish context = () =>
        {
            id = new CollaboratorId(Guid.NewGuid());
            events = new List<IEvent>();
            events.Add(new CollaboratorRenamed(id, "first", "last"));
        };

        Because of = () => expectedException = Catch.Exception(() => AggregateRootFactory.Build<Collaborator>(events));

        It an__AggregateRootException__should_be_thrown = () => expectedException.ShouldBeOfType<AggregateRootException>();

        static CollaboratorId id;
        static List<IEvent> events;
        static Exception expectedException;
    }
}
