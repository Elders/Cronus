using Machine.Specifications;

namespace Elders.Cronus.Projections
{

    [Subject("ProjectionVersions")]
    public class When_adding_a_live_version_to_a_versions_collectionth
    {
        Establish context = () =>
        {
            versions = new ProjectionVersions();

            var buildingVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 1, "hash");
            buildingVersion2 = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");

            versions.Add(buildingVersion);

            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 3, "hash");
        };

        Because of = () => versions.Add(buildingVersion2);

        It should_have_next_version = () => versions.GetNext().ShouldEqual(nextVersion);
        It should_have_live_version = () => versions.GetLive().ShouldBeNull();

        It should_not_be__canceled__ = () => versions.IsCanceled(buildingVersion2).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(buildingVersion2).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
        static ProjectionVersion buildingVersion2;
    }
}
