using Elders.Cronus.Projections;
using Machine.Specifications;

namespace Elders.Cronus.Tests.Projections
{
    [Subject("Projections")]
    public class When_projection_version_is_added_twice
    {
        Establish context = () =>
        {
            building = new ProjectionVersion("buildingId", ProjectionStatus.Building, 1, "buildingHash");

            versions = new ProjectionVersions();
            versions.Add(building);
        };

        Because of = () => versions.Add(building);

        It should_have_one_version = () => versions.Count.ShouldEqual(1);

        static ProjectionVersions versions;
        static ProjectionVersion building;
    }
}
