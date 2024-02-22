using Elders.Cronus.EventStore.Players;
using Elders.Cronus.Projections.Versioning;
using Elders.Cronus.Testing;
using Machine.Specifications;
using System;
using System.Linq;

namespace Elders.Cronus.Projections;

[Subject("Projections")]
public class When_projection_version_with_status_building_is_outside_of_the_timebox
{
    Establish context = () =>
    {
        string projectionName = "26ff30da-bf0d-4b3b-874f-e305d5870741";
        hash = "e4glzg";
        var id = new ProjectionVersionManagerId(projectionName, "elders");

        ar = Aggregate<ProjectionVersionManager>
            .FromHistory(stream => stream
                .AddEvent(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.New, 1, hash), new ReplayEventsOptions(), new VersionRequestTimebox(DateTime.Parse("2018-11-28T11:07:47.9657464Z"), DateTime.Parse("2018-11-29T11:07:47.9657464Z"))))
                .AddEvent(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.New, 2, hash), new ReplayEventsOptions(), new VersionRequestTimebox(DateTime.Parse("2018-11-28T11:28:47.8453615Z"), DateTime.Parse("2018-11-29T11:28:47.8453615Z"))))
                .AddEvent(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.New, 3, hash), new ReplayEventsOptions(), new VersionRequestTimebox(DateTime.Parse("2018-11-28T11:31:01.0982545Z"), DateTime.Parse("2018-11-29T11:31:01.0982545Z"))))
                .AddEvent(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.New, 4, hash), new ReplayEventsOptions(), new VersionRequestTimebox(DateTime.Parse("2018-11-28T14:41:44.5082787Z"), DateTime.Parse("2018-11-29T14:41:44.5082787Z"))))
                .AddEvent(new ProjectionVersionRequested(id, new ProjectionVersion(projectionName, ProjectionStatus.New, 5, hash), new ReplayEventsOptions(), new VersionRequestTimebox(DateTime.Parse("2018-11-28T14:43:35.834907Z"), DateTime.Parse("2018-11-29T14:43:35.834907Z"))))
                .AddEvent(new ProjectionVersionRequestTimedout(id, new ProjectionVersion(projectionName, ProjectionStatus.Timedout, 1, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T10:58:41.4293469Z"), DateTime.Parse("2018-11-29T10:58:41.4293469Z"))))
                .AddEvent(new ProjectionVersionRequestTimedout(id, new ProjectionVersion(projectionName, ProjectionStatus.Timedout, 4, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T14:41:44.5082787Z"), DateTime.Parse("2018-11-29T14:41:44.5082787Z"))))
                .AddEvent(new ProjectionVersionRequestTimedout(id, new ProjectionVersion(projectionName, ProjectionStatus.Timedout, 5, hash), new VersionRequestTimebox(DateTime.Parse("2018-11-28T14:43:35.834907Z"), DateTime.Parse("2018-11-29T14:43:35.834907Z"))))
            );
    };

    Because of = () => ar.Replay(hash, new MarkupInterfaceProjectionVersioningPolicy(), new ReplayEventsOptions());

    It should_timeout_the_obsolete_building_versions = () => ar.PublishedEvents<ProjectionVersionRequestTimedout>().Count().ShouldEqual(2);

    static ProjectionVersionManager ar;
    static string hash;
}
