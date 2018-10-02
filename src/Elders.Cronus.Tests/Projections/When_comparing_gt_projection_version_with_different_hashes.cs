using System;
using Elders.Cronus.Projections;
using Machine.Specifications;

namespace Elders.Cronus.Tests.Projections
{
    [Subject("Projections")]
    public class When_comparing_gt_projection_version_with_different_hashes
    {
        Establish context = () =>
        {
            lower = new ProjectionVersion("compare_gt", ProjectionStatus.Live, 1, "compare_gt_hash");
            higher = new ProjectionVersion("compare_gt", ProjectionStatus.Live, 2, "ops");
        };

        Because of = () => result = lower > higher;

        It should_be_able_to_compare_with_gt = () => result.ShouldBeFalse();

        static bool result;
        static ProjectionVersion lower;
        static ProjectionVersion higher;
    }
}
