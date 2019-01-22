using Machine.Specifications;

namespace Elders.Cronus.Projections
{
    [Subject("ProjectionVersions")]
    public class When_having_empty_projection_versions
    {
        Because of = () => versions = new ProjectionVersions();

        It should_not_have_next_version = () => versions.GetNext().ShouldBeNull();
        It should_not_have_live_version = () => versions.GetLive().ShouldBeNull();

        static ProjectionVersions versions;
    }
}
