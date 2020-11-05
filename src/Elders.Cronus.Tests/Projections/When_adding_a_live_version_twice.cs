using Elders.Cronus.Projections.Versioning;
using Machine.Specifications;

namespace Elders.Cronus.Projections
{
    [Subject("ProjectionVersions")]
    public class When_adding_a_live_version_twice
    {
        Establish context = () =>
        {
            var initialLiveVersion = new ProjectionVersion("projectionName", ProjectionStatus.Live, 1, "hash");
            versions = new ProjectionVersions(initialLiveVersion);

            version = new ProjectionVersion("projectionName", ProjectionStatus.Live, 2, "hash");
            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 3, "hash");
        };

        Because of = () => versions.Add(version);

        It should_have_next_version = () => versions.GetNext(new MarkupInterfaceProjectionVersioningPolicy(), "hash").ShouldEqual(nextVersion);
        It should_have_live_version = () => versions.GetLive().ShouldNotBeNull();
        It should_have_correct_live_version = () => versions.GetLive().ShouldEqual(version);

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }
}
