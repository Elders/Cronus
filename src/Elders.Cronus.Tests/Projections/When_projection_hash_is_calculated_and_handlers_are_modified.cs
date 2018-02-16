using Machine.Specifications;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.Tests.Projections
{
    [Subject("Projections")]
    public class When_projection_hash_is_calculated_and_handlers_are_modified
    {
        Establish context = () =>
        {
            hasher = new ProjectionHasher();
            hash = hasher.CalculateHash(typeof(TestProjection));
        };

        Because of = () => shuffledHash = hasher.CalculateHash(typeof(TestProjectionModified));

        It should_have_hash = () => shuffledHash.ShouldNotBeNull();

        It should_have_valid_hash = () => shuffledHash.ShouldNotEqual(hash);

        static ProjectionHasher hasher;
        static string shuffledHash;
        static string hash;
    }
}
