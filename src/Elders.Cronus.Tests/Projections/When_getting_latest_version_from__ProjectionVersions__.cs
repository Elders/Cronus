using Elders.Cronus.Projections;
using Machine.Specifications;

namespace Elders.Cronus.Tests.Projections
{
    [Subject("Projections")]
    public class When_getting_latest_version_from__ProjectionVersions__
    {
        Establish context = () =>
        {
            versions = new ProjectionVersions();
            latest = new ProjectionVersion("latestId", ProjectionStatus.Building, 1, "latestHash");
            versions.Add(latest);
        };

        Because of = () => version = versions.GetLatest();

        It should_have_version = () => versions.ShouldNotBeNull();

        It should_return_valid_version = () => (version == latest).ShouldBeTrue();

        It should_not_have_canceled_version = () => versions.IsCanceled(latest).ShouldBeFalse();

        static ProjectionVersions versions;
        static ProjectionVersion version;
        static ProjectionVersion latest;
    }
}
