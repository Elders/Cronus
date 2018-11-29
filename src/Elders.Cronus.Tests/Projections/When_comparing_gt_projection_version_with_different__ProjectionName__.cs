using System;
using Elders.Cronus.Projections;
using Machine.Specifications;

namespace Elders.Cronus.Tests.Projections
{
    [Subject("Projections")]
    public class When_comparing_gt_projection_version_with_different__ProjectionName__
    {
        Establish context = () =>
        {
            lower = new ProjectionVersion("compare_gt", ProjectionStatus.Live, 1, "compare_gt_hash");
            higher = new ProjectionVersion("ops", ProjectionStatus.Live, 1, "compare_gt_hash");
        };

        Because of = () => exception = Catch.Exception(() => lower > higher);

        It should_fail = () => exception.ShouldNotBeNull();

        static ProjectionVersion lower;
        static ProjectionVersion higher;
        static Exception exception;
    }
}
