using Elders.Cronus.Projections.Versioning;
using Machine.Specifications;

namespace Elders.Cronus.Projections
{
    [Subject("ProjectionVersions")]
    public class When_adding_a_live_version_to_a_versions_collection
    {
        Establish context = () =>
        {
            var notPresentVersion = new ProjectionVersion("projectionName", ProjectionStatus.NotPresent, 1, "hash");
            var buldingVersion = new ProjectionVersion("projectionName", ProjectionStatus.Replaying, 1, "hash");
            var canceledVersion = new ProjectionVersion("projectionName", ProjectionStatus.Canceled, 1, "hash");
            var canceledVersion2 = new ProjectionVersion("projectionName", ProjectionStatus.Canceled, 1, "hash");
            var timedoutVersion = new ProjectionVersion("projectionName", ProjectionStatus.Timedout, 1, "hash");
            liveVersion = new ProjectionVersion("projectionName", ProjectionStatus.Live, 1, "hash");

            versions = new ProjectionVersions(notPresentVersion);

            versions.Add(buldingVersion);
            versions.Add(canceledVersion);
            versions.Add(canceledVersion2);
            versions.Add(timedoutVersion);

            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Replaying, 2, "hash");
        };

        Because of = () => versions.Add(liveVersion);

        It should_have_next_version = () => versions.GetNext(new MarkupInterfaceProjectionVersioningPolicy(), "hash").ShouldEqual(nextVersion);
        It should_have_live_version = () => versions.GetLive().ShouldNotBeNull();
        It should_have_correct_live_version = () => versions.GetLive().ShouldEqual(liveVersion);

        It should_not_be__canceled__ = () => versions.IsCanceled(liveVersion).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdated(liveVersion).ShouldBeFalse();

        static ProjectionVersion liveVersion;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }
}
