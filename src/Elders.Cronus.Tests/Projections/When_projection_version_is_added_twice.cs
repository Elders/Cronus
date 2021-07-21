using Elders.Cronus.Projections;
using Machine.Specifications;
using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Tests.Projections
{
    [Subject("Projections")]
    public class When_projection_version_is_added_twice
    {
        Establish context = () =>
        {
            building = new ProjectionVersion("buildingId", ProjectionStatus.Replaying, 1, "buildingHash");
            versions = new ProjectionVersions(building);
        };

        Because of = () => versions.Add(building);

        It should_have_one_version = () => versions.Count.ShouldEqual(1);

        static ProjectionVersions versions;
        static ProjectionVersion building;
    }

    //[Subject("Projections")]
    //public class When_checking_if_projection_version_is_replayable_and_it_has_not_been_marked_by_interface
    //{
    //    Establish context = () =>
    //    {
    //        //projectionName = typeof(TestProjection).GetAttrubuteValue<DataContractAttribute, string>(x => x.Name); gurmej
    //        replayableVersion = new ProjectionVersion("05a82e14-3bcd-4e0e-a725-65f3d3a0ee0e", ProjectionStatus.Live, 1, "buildingHash"); // test projection
    //    };

    //    Because of = () => result = replayableVersion.IsReplayable();

    //    It should_not_be_replayable = () => result.ShouldBeFalse();

    //    static ProjectionVersion replayableVersion;
    //    static string projectionName;
    //    static bool result;
    //}

    //[Subject("Projections")]
    //public class When_checking_if_projection_version_is_replayable_and_it_has1_been_marked_by_interface
    //{
    //    Establish context = () =>
    //    {
    //        //projectionName = typeof(NonReplayableProjection).GetAttrubuteValue<DataContractAttribute, string>(x => x.Name); gurmej
    //        nonReplayableVersion = new ProjectionVersion("INonReplayableProjection", ProjectionStatus.Live, 1, "buildingHash");
    //    };

    //    Because of = () => result = nonReplayableVersion.IsReplayable();

    //    It should_be_replayable = () => result.ShouldBeTrue();

    //    static ProjectionVersion nonReplayableVersion;
    //    static string projectionName;
    //    static bool result;
    //}
}
