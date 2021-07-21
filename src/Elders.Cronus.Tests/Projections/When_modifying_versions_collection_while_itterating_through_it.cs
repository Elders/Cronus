using Elders.Cronus.Projections;
using Machine.Specifications;

namespace Elders.Cronus.Tests.Projections
{
    [Subject("Projections")]
    public class When_modifying_versions_collection_while_itterating_through_it
    {
        Establish context = () =>
        {
            var building = new ProjectionVersion("buildingId", ProjectionStatus.Replaying, 1, "buildingHash");

            versions = new ProjectionVersions(building);

            another = new ProjectionVersion("buildingId", ProjectionStatus.Canceled, 1, "buildingHash");
        };

        Because of = () =>
        {
            foreach (var item in versions)
            {
                versions.Add(another);
            }
        };

        It should_be_possible_to_add_new_versions = () => versions.Count.ShouldEqual(1);

        It should_have_the_new_version = () => versions.IsCanceled(another).ShouldBeTrue();

        static ProjectionVersions versions;
        static ProjectionVersion another;
    }
}
