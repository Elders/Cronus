using Machine.Specifications;

namespace Elders.Cronus.Projections
{
    [Subject("ProjectionVersions")]
    public class When_adding_a_building_version_to_a_live_collection
    {
        Establish context = () =>
        {
            initialLiveVersion = new ProjectionVersion("projectionName", ProjectionStatus.Live, 1, "hash");
            versions = new ProjectionVersions();
            versions.Add(initialLiveVersion);
            version = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");
        };

        Because of = () => versions.Add(version);

        It should_have_next_version = () => versions.GetNext().ShouldNotEqual(version);
        It should_have_live_version = () => versions.GetLive().ShouldNotBeNull();
        It should_have_correct_live_version = () => versions.GetLive().ShouldEqual(initialLiveVersion);

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion initialLiveVersion;
        static ProjectionVersion version;
        static ProjectionVersions versions;
    }
}
