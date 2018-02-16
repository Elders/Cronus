using System;
using System.Collections.Generic;
using System.Text;
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
}
