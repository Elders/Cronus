using Machine.Specifications;

namespace Elders.Cronus.Projections
{
    [Subject("ProjectionVersions")]
    public class When_adding_a_pending_version
    {
        Establish context = () =>
        {
            version = new ProjectionVersion("projectionName", ProjectionStatus.Pending, 1, "hash");
            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");
            versions = new ProjectionVersions();
        };

        Because of = () => versions.Add(version);

        It should_have_next_version = () => versions.GetNext().ShouldEqual(nextVersion);
        It should_not_have_live_version = () => versions.GetLive().ShouldBeNull();

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_not_be__present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }
}
