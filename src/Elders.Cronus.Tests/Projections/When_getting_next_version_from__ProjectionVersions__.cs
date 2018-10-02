using Elders.Cronus.Projections;
using Machine.Specifications;

namespace Elders.Cronus.Tests.Projections
{
    [Subject("Projections")]
    public class When_getting_next_version_from__ProjectionVersions__
    {
        Establish context = () =>
        {
            versions = new ProjectionVersions();
            var latest = new ProjectionVersion("latestId", ProjectionStatus.Building, 1, "latestHash");
            expected = new ProjectionVersion("latestId", ProjectionStatus.Building, 2, "latestHash");
            versions.Add(expected);
        };

        Because of = () => next = versions.GetLatest();

        It should_have_version = () => versions.ShouldNotBeNull();

        It should_return_valid_version = () => (next == expected).ShouldBeTrue();

        static ProjectionVersions versions;
        static ProjectionVersion next;
        static ProjectionVersion expected;
    }
}
