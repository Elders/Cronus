using Elders.Cronus.Projections.Versioning;
using Machine.Specifications;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections
{
    [Subject("ProjectionVersions")]
    public class When_adding_a_building_verrsion
    {
        Establish context = () =>
        {
            version = new ProjectionVersion("projectionName", ProjectionStatus.Building, 1, "hash");
            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");
        };

        Because of = () => versions = new ProjectionVersions(version);

        It should_have_next_version = () => versions.GetNext(new MarkupInterfaceProjectionVersioningPolicy()).ShouldEqual(nextVersion);
        It should_not_have_live_version = () => versions.GetLive().ShouldBeNull();

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }

    [Subject("ProjectionVersions")]
    public class When_getting_next_version_to_non_versionable_projection
    {
        Establish context = () =>
        {
            MessageInfo.GetContractId(typeof(NonVersionableProjection));

            version = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Building, 1, "hash");
            nextVersion = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Building, 1, "hash");
        };

        Because of = () => versions = new ProjectionVersions(version);

        It should_have_next_version = () => versions.GetNext(new MarkupInterfaceProjectionVersioningPolicy()).ShouldEqual(nextVersion);
        It should_not_have_live_version = () => versions.GetLive().ShouldBeNull();

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }

    [Subject("ProjectionVersions")]
    public class When_adding_a_building_verrsion_to_non_versionable_projection
    {
        Establish context = () =>
        {
            MessageInfo.GetContractId(typeof(NonVersionableProjection));

            version = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Live, 1, "hash");
            versions = new ProjectionVersions(version);

            var rebuildVersion = versions.GetNext(new MarkupInterfaceProjectionVersioningPolicy());
            versions.Add(rebuildVersion);

            nextVersion = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Live, 1, "hash");
        };

        Because of = () => versions.Add(nextVersion);

        It should_not_have_live_version = () => versions.GetLive().ShouldEqual(version);

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }

    [Subject("ProjectionVersions")]
    public class When_switching_a_projection_to_non_versionable
    {
        Establish context = () =>
        {
            MessageInfo.GetContractId(typeof(NonVersionableProjection));

            var build1 = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Building, 1, "hash");
            var live1 = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Live, 1, "hash");
            var build2 = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Building, 2, "hash");
            var live2 = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Live, 2, "hash");
            var build3 = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Building, 3, "hash");
            liveVersion = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Live, 3, "hash");
            versions = new ProjectionVersions(new[] { build1, live1, build2, live2, build3, liveVersion });

            nextVersion = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Building, 3, "hash");
        };

        Because of = () => version = versions.GetNext(new MarkupInterfaceProjectionVersioningPolicy());

        It should_have_next_version = () => version.ShouldEqual(nextVersion);
        It should_have_live_version = () => versions.GetLive().ShouldEqual(liveVersion);

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_not_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion liveVersion;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }

    [Subject("ProjectionVersions")]
    public class When_switching_a_projection_to_non_versionable1
    {
        Establish context = () =>
        {
            MessageInfo.GetContractId(typeof(NonVersionableProjection));

            var build1 = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Building, 1, "hash");
            var live1 = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Live, 1, "hash");
            var build2 = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Building, 2, "hash");
            var live2 = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Live, 2, "hash");
            var build3 = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Building, 3, "hash");
            liveVersion = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Live, 3, "hash");
            versions = new ProjectionVersions(new[] { build1, live1, build2, live2, build3, liveVersion });

            nextVersion = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Building, 3, "hash");
            versions.Add(nextVersion);

            version = new ProjectionVersion("NonVersionableProjection", ProjectionStatus.Live, 3, "hash");
        };

        Because of = () => versions.Add(liveVersion);

        It should_have_next_version = () => versions.GetNext(new MarkupInterfaceProjectionVersioningPolicy()).ShouldEqual(nextVersion);
        It should_have_live_version = () => versions.GetLive().ShouldEqual(liveVersion);

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_not_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion liveVersion;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }

    [DataContract(Name = "NonVersionableProjection")]
    public class NonVersionableProjection : IProjection, INonVersionableProjection { }
}
