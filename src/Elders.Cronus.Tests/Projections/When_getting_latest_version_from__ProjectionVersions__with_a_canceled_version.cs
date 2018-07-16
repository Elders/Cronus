using Elders.Cronus.Projections;
using Machine.Specifications;

namespace Elders.Cronus.Tests.Projections
{
    [Subject("Projections")]
    public class When_getting_latest_version_from__ProjectionVersions__with_a_canceled_version
    {
        Establish context = () =>
        {
            versions = new ProjectionVersions();
            latest = new ProjectionVersion("latestId", ProjectionStatus.Building, 1, "latestHash");
            var canceled = new ProjectionVersion("latestId", ProjectionStatus.Canceled, 1, "latestHash");
            versions.Add(latest);
            versions.Add(canceled);
        };

        Because of = () => version = versions.GetLatest();

        It should_have_canceled_version = () => versions.IsCanceled(latest).ShouldBeTrue();

        static ProjectionVersions versions;
        static ProjectionVersion version;
        static ProjectionVersion latest;
    }
}
