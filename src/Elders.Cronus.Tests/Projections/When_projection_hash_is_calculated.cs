using Machine.Specifications;
using Elders.Cronus.Projections.Versioning;

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
