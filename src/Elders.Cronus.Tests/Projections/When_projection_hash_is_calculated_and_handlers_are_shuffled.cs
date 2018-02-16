using Machine.Specifications;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.Tests.Projections
{
    [Subject("Projections")]
    public class When_projection_hash_is_calculated_and_handlers_are_shuffled
    {
        Establish context = () =>
        {
            hasher = new ProjectionHasher();
            hash = hasher.CalculateHash(typeof(TestProjection));
        };

        Because of = () => shuffledHash = hasher.CalculateHash(typeof(TestProjectionShuffled));

        It should_have_hash = () => shuffledHash.ShouldNotBeNull();

        It should_have_valid_hash = () => shuffledHash.ShouldEqual(hash);

        static ProjectionHasher hasher;
        static string shuffledHash;
        static string hash;
    }
}
