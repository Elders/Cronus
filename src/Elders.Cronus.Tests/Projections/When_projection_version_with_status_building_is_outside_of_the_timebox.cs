using Elders.Cronus.Projections.Versioning;
using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elders.Cronus.Projections
{
    [Subject("Projections")]
    public class When_projection_version_with_status_building_is_outside_of_the_timebox
    {
        Establish context = () =>
        {
            string projectionName = "26ff30da-bf0d-4b3b-874f-e305d5870741";
            hash = "e4glzg";
            var id = new ProjectionVersionManagerId(projectionName, "elders");

            ar = Aggregate<ProjectionVersionManager>
                .FromHistory()
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Building, 1, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T11:07:47.9657464Z"), DateTime.Parse("2018-11-29T11:07:47.9657464Z"))))
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Building, 2, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T11:28:47.8453615Z"), DateTime.Parse("2018-11-29T11:28:47.8453615Z"))))
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Building, 3, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T11:31:01.0982545Z"), DateTime.Parse("2018-11-29T11:31:01.0982545Z"))))
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Building, 4, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T14:41:44.5082787Z"), DateTime.Parse("2018-11-29T14:41:44.5082787Z"))))
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Building, 5, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T14:43:35.834907Z"), DateTime.Parse("2018-11-29T14:43:35.834907Z"))))
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Timedout, 1, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T10:58:41.4293469Z"), DateTime.Parse("2018-11-29T10:58:41.4293469Z"))))
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Timedout, 4, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T14:41:44.5082787Z"), DateTime.Parse("2018-11-29T14:41:44.5082787Z"))))
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Timedout, 5, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T14:43:35.834907Z"), DateTime.Parse("2018-11-29T14:43:35.834907Z"))))
                .Build();
        };

        Because of = () => ar.Replay(hash);

        It should_timeout_the_obsolete_building_versions = () => ar.PublishedEvents<ProjectionVersionRequestTimedout>().Count().ShouldEqual(2);

        static ProjectionVersionManager ar;
        static string hash;
    }

    public static class Aggregate<T> where T : IAggregateRoot
    {
        public static AggregateRootHistory<T> FromHistory()
        {
            return new AggregateRootHistory<T>();
        }

        public class AggregateRootHistory<TT> where TT : IAggregateRoot
        {
            public AggregateRootHistory()
            {
                Events = new List<IEvent>();
            }

            public List<IEvent> Events { get; set; }

            public AggregateRootHistory<TT> Event(IEvent @event)
            {
                Events.Add(@event);
                return this;
            }

            public TT Build()
            {
                var instance = (TT)Activator.CreateInstance(typeof(TT), true);
                instance.ReplayEvents(Events, 1);
                return instance;
            }
        }
    }

    public static class AggregateRootExtensions
    {
        public static T PublishedEvent<T>(this IAggregateRoot root) where T : IEvent
        {
            var @event = root.UncommittedEvents.SingleOrDefault(x => x is T);
            if (ReferenceEquals(@event, null) || @event.Equals(default(T)))
                return default(T);
            return (T)@event;
        }

        public static IEnumerable<IEvent> PublishedEvents<T>(this IAggregateRoot root) where T : IEvent
        {
            var events = root.UncommittedEvents.Where(x => x is T);
            return events;
        }

        public static bool IsEventPublished<T>(this IAggregateRoot root) where T : IEvent
        {
            return ReferenceEquals(default(T), PublishedEvent<T>(root)) == false;
        }

        public static bool HasNewEvents(this IAggregateRoot root)
        {
            return root.UncommittedEvents.Any();
        }

        public static T RootState<T>(this AggregateRoot<T> root) where T : IAggregateRootState, new()
        {
            return (T)(root as IHaveState<IAggregateRootState>).State;
        }
    }
}
