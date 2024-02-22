using Elders.Cronus.Projections;
using Machine.Specifications;

namespace Elders.Cronus.Tests.Projections;

[Subject("Projections")]
public class When_comparing_lt_projection_version_with_same_revisions
{
    Establish context = () =>
    {
        lower = new ProjectionVersion("compare_lt", ProjectionStatus.Live, 1, "compare_lt_hash");
        higher = new ProjectionVersion("compare_lt", ProjectionStatus.Live, 1, "compare_lt_hash");
    };

    Because of = () => result = lower < higher;

    It should_be_able_to_compare_with_lt_and_return_false = () => result.ShouldBeFalse();

    static bool result;
    static ProjectionVersion lower;
    static ProjectionVersion higher;
}
