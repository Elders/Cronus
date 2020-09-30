using System;
using Elders.Cronus.Projections;
using Machine.Specifications;

namespace Elders.Cronus.Tests.Projections
{
    [Subject("Projections")]
    public class When_projection_version_with_different_contractId_is_added
    {
        Establish context = () =>
        {
            var building = new ProjectionVersion("buildingId", ProjectionStatus.Building, 1, "buildingHash");
            versions = new ProjectionVersions(building);

            differentId = new ProjectionVersion("differentId", ProjectionStatus.Building, 1, "buildingHash");
        };

        Because of = () => exception = Catch.Exception(() => versions.Add(differentId));

        It should_throw_an_ArgumentException = () => exception.ShouldNotBeNull();

        static ProjectionVersions versions;
        static ProjectionVersion differentId;
        static Exception exception;
    }
}
