using Elders.Cronus.Projections.Versioning;
using Machine.Specifications;
using System;
using System.Collections.Generic;

namespace Elders.Cronus.Projections
{
    [Subject("Projections")]
    public class When_projection_state
    {
        Establish context = () =>
        {
            string projectionName = "26ff30da-bf0d-4b3b-874f-e305d5870741";
            hash = "lm0bzg";
            var id = new ProjectionVersionManagerId(projectionName, "elders");

            projectionHistory = Projection<ProjectionVersionsHandler>
                .FromHistory()
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Replaying, 1, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T11:07:47.9657464Z"), DateTime.Parse("2018-11-29T11:07:47.9657464Z"))))
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Replaying, 2, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T11:28:47.8453615Z"), DateTime.Parse("2018-11-29T11:28:47.8453615Z"))))
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Replaying, 3, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T11:31:01.0982545Z"), DateTime.Parse("2018-11-29T11:31:01.0982545Z"))))
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Replaying, 4, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T14:41:44.5082787Z"), DateTime.Parse("2018-11-29T14:41:44.5082787Z"))))
                .Event(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.Replaying, 5, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T14:43:35.834907Z"), DateTime.Parse("2018-11-29T14:43:35.834907Z"))))
                .Event(new ProjectionVersionRequestTimedout(id, new ProjectionVersion(projectionName, ProjectionStatus.Timedout, 1, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T10:58:41.4293469Z"), DateTime.Parse("2018-11-29T10:58:41.4293469Z"))))
                .Event(new ProjectionVersionRequestTimedout(id, new ProjectionVersion(projectionName, ProjectionStatus.Timedout, 4, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T14:41:44.5082787Z"), DateTime.Parse("2018-11-29T14:41:44.5082787Z"))))
                .Event(new ProjectionVersionRequestTimedout(id, new ProjectionVersion(projectionName, ProjectionStatus.Timedout, 5, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T14:43:35.834907Z"), DateTime.Parse("2018-11-29T14:43:35.834907Z"))));
        };

        Because of = () => projection = projectionHistory.Build();

        It should_timeout_the_obsolete_building_versions = () => projection.State.ShouldNotBeNull();

        static ProjectionVersionsHandler projection;
        static string hash;
        static Projection<ProjectionVersionsHandler>.ProjectionHistory projectionHistory;
    }

    public static class Projection<T> where T : IProjectionDefinition
    {
        public static ProjectionHistory FromHistory()
        {
            return new ProjectionHistory();
        }

        public class ProjectionHistory
        {
            public ProjectionHistory()
            {
                Events = new List<IEvent>();
            }

            public List<IEvent> Events { get; private set; }

            public ProjectionHistory Event(IEvent @event)
            {
                if (@event is null) throw new ArgumentNullException(nameof(@event));

                Events.Add(@event);
                return this;
            }

            public T Build()
            {
                var instance = (T)Activator.CreateInstance(typeof(T), true);
                instance.ReplayEvents(Events);
                return instance;
            }
        }
    }

    //public static class AggregateRootExtensions
    //{
    //    public static T State<T>(this IProjectionDefinition projection) where T : class, new()
    //    {
    //        return (T)(projection.State);
    //    }
    //}
}
