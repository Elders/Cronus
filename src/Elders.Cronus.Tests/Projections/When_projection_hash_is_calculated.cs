using Machine.Specifications;
using Elders.Cronus.Projections.Versioning;
using Machine.Specifications.Model;

namespace Elders.Cronus.Tests.Projections;

[Subject("Projections")]
public class When_projection_hash_is_calculated
{
    Because of = () => hash = new ProjectionHasher().CalculateHash(typeof(TestProjection));

    It should_have_hash = () => hash.ShouldNotBeNull();

    It should_have_valid_hash = () => hash.ShouldEqual(persistentHash);

    static string hash;
    static string persistentHash = "d5sgqq";
}

[Subject("Projections")]
public class When_projection_handlers
{
    Establish context = () =>
    {
        hash_asc = new ProjectionHasher().CalculateHash(typeof(TestProjectionHandlersAsc));
    };

    Because of = () => hash_desc = new ProjectionHasher().CalculateHash(typeof(TestProjectionHandlersDesc));

    It should_have_hash = () => hash_desc.ShouldNotBeNull();

    It hash_should_be_the_same_when_events_are_shuffled = () => hash_asc.ShouldEqual(hash_desc);

    It should_have_correct_hash = () => hash_desc.ShouldEqual(persistentHash);

    static string hash_asc;
    static string hash_desc;
    static string persistentHash = "cpbsg";
}
