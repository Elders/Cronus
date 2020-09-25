using Elders.Cronus.Projections.Versioning;
using Machine.Specifications;

namespace Elders.Cronus.Projections
{
    [Subject("ProjectionVersions")]
    public class When_adding_a_live_version
    {
        Establish context = () =>
        {
            version = new ProjectionVersion("projectionName", ProjectionStatus.Live, 1, "hash");
            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");
        };

        Because of = () => versions = new ProjectionVersions(version);

        It should_have_next_version = () => versions.GetNext(new MarkupInterfaceProjectionVersioningPolicy()).ShouldEqual(nextVersion);
        It should_have_correct_live_version = () => versions.GetLive().ShouldEqual(version);
        It should_have_live_version = () => versions.GetLive().ShouldNotBeNull();

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }
}
