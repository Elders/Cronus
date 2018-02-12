using System;
using Elders.Cronus.Projections;
using Machine.Specifications;
using System.Runtime.Serialization;
using System.Linq;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.Tests.Projections
{
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

    [DataContract(Name = "05a82e14-3bcd-4e0e-a725-65f3d3a0ee0e")]
    public class TestProjection : ProjectionDefinition<TestProjectionState, TestProjectionId>,
        IEventHandler<TestEvent1>,
        IEventHandler<TestEvent2>,
        IEventHandler<TestEvent3>
    {
        public void Handle(TestEvent1 @event) { }

        public void Handle(TestEvent2 @event) { }

        public void Handle(TestEvent3 @event) { }

        [DataMember(Order = 1)]
        public string Text { get; set; }
    }

    [DataContract(Name = "05a82e14-3bcd-4e0e-a725-65f3d3a0ee0e")]
    public class TestProjectionShuffled : ProjectionDefinition<TestProjectionState, TestProjectionId>,
        IEventHandler<TestEvent2>,
        IEventHandler<TestEvent3>,
        IEventHandler<TestEvent1>
    {
        public void Handle(TestEvent1 @event) { }

        public void Handle(TestEvent2 @event) { }

        public void Handle(TestEvent3 @event) { }

        [DataMember(Order = 1)]
        public string Text { get; set; }
    }

    [DataContract(Name = "05a82e14-3bcd-4e0e-a725-65f3d3a0ee0e")]
    public class TestProjectionModified : ProjectionDefinition<TestProjectionState, TestProjectionId>,
        IEventHandler<TestEvent2>,
        IEventHandler<TestEvent1>
    {
        public void Handle(TestEvent1 @event) { }

        public void Handle(TestEvent2 @event) { }

        [DataMember(Order = 1)]
        public string Text { get; set; }
    }

    public class TestProjectionState
    {

    }

    public class TestProjectionId : StringTenantId
    {

    }

    [DataContract(Name = "25061980-5057-475f-b734-2c4a6b52286f")]
    public class TestEvent1 : IEvent { }

    [DataContract(Name = "833bedee-0109-402b-81de-29986bd46221")]
    public class TestEvent2 : IEvent { }

    [DataContract(Name = "7898a318-c8e5-4be5-b1e3-13c4f5da28d5")]
    public class TestEvent3 : IEvent { }
}
