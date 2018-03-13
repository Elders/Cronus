using System;
using System.Linq;
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

    [Subject("Projections")]
    public class When_projection_version_with_different_contractId_is_added
    {
        Establish context = () =>
        {
            versions = new ProjectionVersions();
            var building = new ProjectionVersion("buildingId", ProjectionStatus.Building, 1, "buildingHash");
            versions.Add(building);

            differentId = new ProjectionVersion("differentId", ProjectionStatus.Building, 1, "buildingHash");
        };

        Because of = () => exception = Catch.Exception(() => versions.Add(differentId));

        It should_throw_an_ArgumentException = () => exception.ShouldBeOfExactType<ArgumentException>();

        static ProjectionVersions versions;
        static ProjectionVersion differentId;
        static Exception exception;
    }

    [Subject("Projections")]
    public class When_modifying_versions_collection_while_itterating_through_it
    {
        Establish context = () =>
        {
            var building = new ProjectionVersion("buildingId", ProjectionStatus.Building, 1, "buildingHash");

            versions = new ProjectionVersions();
            versions.Add(building);

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

        It should_have_the_new_version = () => versions.Single().Status.ShouldEqual(ProjectionStatus.Canceled);

        static ProjectionVersions versions;
        static ProjectionVersion another;
    }
}
