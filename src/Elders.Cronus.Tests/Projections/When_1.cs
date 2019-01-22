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

    [Subject("ProjectionVersions")]
    public class When_adding_a_pending_version
    {
        Establish context = () =>
        {
            version = new ProjectionVersion("projectionName", ProjectionStatus.Pending, 1, "hash");
            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");
            versions = new ProjectionVersions();
        };

        Because of = () => versions.Add(version);

        It should_have_next_version = () => versions.GetNext().ShouldEqual(nextVersion);
        It should_not_have_live_version = () => versions.GetLive().ShouldBeNull();

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_not_be__present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }


    [Subject("ProjectionVersions")]
    public class When_adding_a_building_verrsion
    {
        Establish context = () =>
        {
            version = new ProjectionVersion("projectionName", ProjectionStatus.Building, 1, "hash");
            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");
            versions = new ProjectionVersions();
        };

        Because of = () => versions.Add(version);

        It should_have_next_version = () => versions.GetNext().ShouldEqual(nextVersion);
        It should_not_have_live_version = () => versions.GetLive().ShouldBeNull();

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }

    [Subject("ProjectionVersions")]
    public class When_adding_a_canceled_version
    {
        Establish context = () =>
        {
            version = new ProjectionVersion("projectionName", ProjectionStatus.Canceled, 1, "hash");
            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");
            versions = new ProjectionVersions();
        };

        Because of = () => versions.Add(version);

        It should_have_next_version = () => versions.GetNext().ShouldEqual(nextVersion);
        It should_not_have_live_version = () => versions.GetLive().ShouldBeNull();

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeTrue();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }

    [Subject("ProjectionVersions")]
    public class When_adding_a_timedout_version
    {
        Establish context = () =>
        {
            version = new ProjectionVersion("projectionName", ProjectionStatus.Timedout, 1, "hash");
            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");
            versions = new ProjectionVersions();
        };

        Because of = () => versions.Add(version);

        It should_have_next_version = () => versions.GetNext().ShouldEqual(nextVersion);
        It should_not_have_live_version = () => versions.GetLive().ShouldBeNull();

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }

    [Subject("ProjectionVersions")]
    public class When_adding_a_live_version
    {
        Establish context = () =>
        {
            version = new ProjectionVersion("projectionName", ProjectionStatus.Live, 1, "hash");
            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");
            versions = new ProjectionVersions();
        };

        Because of = () => versions.Add(version);

        It should_have_next_version = () => versions.GetNext().ShouldEqual(nextVersion);
        It should_have_correct_live_version = () => versions.GetLive().ShouldEqual(version);
        It should_have_live_version = () => versions.GetLive().ShouldNotBeNull();

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }


    [Subject("ProjectionVersions")]
    public class When_adding_a_live_version_twice
    {
        Establish context = () =>
        {
            var initialLiveVersion = new ProjectionVersion("projectionName", ProjectionStatus.Live, 1, "hash");
            versions = new ProjectionVersions();
            versions.Add(initialLiveVersion);
            version = new ProjectionVersion("projectionName", ProjectionStatus.Live, 2, "hash");
            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 3, "hash");
        };

        Because of = () => versions.Add(version);

        It should_have_next_version = () => versions.GetNext().ShouldEqual(nextVersion);
        It should_have_live_version = () => versions.GetLive().ShouldNotBeNull();
        It should_have_correct_live_version = () => versions.GetLive().ShouldEqual(version);

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion version;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }

    [Subject("ProjectionVersions")]
    public class When_adding_a_building_version_to_a_live_collection
    {
        Establish context = () =>
        {
            initialLiveVersion = new ProjectionVersion("projectionName", ProjectionStatus.Live, 1, "hash");
            versions = new ProjectionVersions();
            versions.Add(initialLiveVersion);
            version = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");
        };

        Because of = () => versions.Add(version);

        It should_have_next_version = () => versions.GetNext().ShouldNotEqual(version);
        It should_have_live_version = () => versions.GetLive().ShouldNotBeNull();
        It should_have_correct_live_version = () => versions.GetLive().ShouldEqual(initialLiveVersion);

        It should_not_be__canceled__ = () => versions.IsCanceled(version).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(version).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion initialLiveVersion;
        static ProjectionVersion version;
        static ProjectionVersions versions;
    }


    [Subject("ProjectionVersions")]
    public class When_adding_a_live_version_to_a_versions_collection
    {
        Establish context = () =>
        {
            versions = new ProjectionVersions();

            var notPresentVersion = new ProjectionVersion("projectionName", ProjectionStatus.NotPresent, 1, "hash");
            var buldingVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 1, "hash");
            var canceledVersion = new ProjectionVersion("projectionName", ProjectionStatus.Canceled, 1, "hash");
            var canceledVersion2 = new ProjectionVersion("projectionName", ProjectionStatus.Canceled, 1, "hash");
            var timedoutVersion = new ProjectionVersion("projectionName", ProjectionStatus.Timedout, 1, "hash");
            liveVersion = new ProjectionVersion("projectionName", ProjectionStatus.Live, 1, "hash");

            versions.Add(notPresentVersion);
            versions.Add(buldingVersion);
            versions.Add(canceledVersion);
            versions.Add(canceledVersion2);
            versions.Add(timedoutVersion);

            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");
        };

        Because of = () => versions.Add(liveVersion);

        It should_have_next_version = () => versions.GetNext().ShouldEqual(nextVersion);
        It should_have_live_version = () => versions.GetLive().ShouldNotBeNull();
        It should_have_correct_live_version = () => versions.GetLive().ShouldEqual(liveVersion);

        It should_not_be__canceled__ = () => versions.IsCanceled(liveVersion).ShouldBeTrue();
        It should_not_be__outdated__ = () => versions.IsOutdatad(liveVersion).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion liveVersion;
        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
    }

    [Subject("ProjectionVersions")]
    public class When_adding_a_live_version_to_a_versions_collectionth
    {
        Establish context = () =>
        {
            versions = new ProjectionVersions();

            var buildingVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 1, "hash");
            buildingVersion2 = new ProjectionVersion("projectionName", ProjectionStatus.Building, 2, "hash");

            versions.Add(buildingVersion);

            nextVersion = new ProjectionVersion("projectionName", ProjectionStatus.Building, 3, "hash");
        };

        Because of = () => versions.Add(buildingVersion2);

        It should_have_next_version = () => versions.GetNext().ShouldEqual(nextVersion);
        It should_have_live_version = () => versions.GetLive().ShouldBeNull();

        It should_not_be__canceled__ = () => versions.IsCanceled(buildingVersion2).ShouldBeFalse();
        It should_not_be__outdated__ = () => versions.IsOutdatad(buildingVersion2).ShouldBeFalse();
        It should_be__not_present__ = () => versions.IsNotPresent().ShouldBeFalse();

        static ProjectionVersion nextVersion;
        static ProjectionVersions versions;
        static ProjectionVersion buildingVersion2;
    }
}
