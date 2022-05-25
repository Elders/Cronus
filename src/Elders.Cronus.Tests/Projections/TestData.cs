using Elders.Cronus.Projections;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.Tests.Projections
{
    [DataContract(Name = "05a82e14-3bcd-4e0e-a725-65f3d3a0ee0e")]
    public class TestProjection : ProjectionDefinition<TestProjectionState, TestProjectionId>,
        IEventHandler<TestEvent1>,
        IEventHandler<TestEvent2>,
        IEventHandler<TestEvent3>
    {
        public Task HandleAsync(TestEvent1 @event) { return Task.CompletedTask; }

        public Task HandleAsync(TestEvent2 @event) { return Task.CompletedTask; }

        public Task HandleAsync(TestEvent3 @event) { return Task.CompletedTask; }

        [DataMember(Order = 1)]
        public string Text { get; set; }
    }

    [DataContract(Name = "05a82e14-3bcd-4e0e-a725-65f3d3a0ee0e")]
    public class TestProjectionShuffled : ProjectionDefinition<TestProjectionState, TestProjectionId>,
        IEventHandler<TestEvent2>,
        IEventHandler<TestEvent3>,
        IEventHandler<TestEvent1>
    {
        public Task HandleAsync(TestEvent1 @event) { return Task.CompletedTask; }

        public Task HandleAsync(TestEvent2 @event) { return Task.CompletedTask; }

        public Task HandleAsync(TestEvent3 @event) { return Task.CompletedTask; }

        [DataMember(Order = 1)]
        public string Text { get; set; }
    }

    [DataContract(Name = "05a82e14-3bcd-4e0e-a725-65f3d3a0ee0e")]
    public class TestProjectionModified : ProjectionDefinition<TestProjectionState, TestProjectionId>,
        IEventHandler<TestEvent2>,
        IEventHandler<TestEvent1>
    {
        public Task HandleAsync(TestEvent1 @event) { return Task.CompletedTask; }

        public Task HandleAsync(TestEvent2 @event) { return Task.CompletedTask; }

        [DataMember(Order = 1)]
        public string Text { get; set; }
    }

    public class TestProjectionState
    {

    }

    public class TestProjectionId : AggregateRootId
    {

    }

    [DataContract(Name = "25061980-5057-475f-b734-2c4a6b52286f")]
    public class TestEvent1 : IEvent { }

    [DataContract(Name = "833bedee-0109-402b-81de-29986bd46221")]
    public class TestEvent2 : IEvent { }

    [DataContract(Name = "7898a318-c8e5-4be5-b1e3-13c4f5da28d5")]
    public class TestEvent3 : IEvent { }

    [DataContract(Name = "NonVersionableProjection")]
    public class NonVersionableProjection : IProjection, INonVersionableProjection { }

    [DataContract(Name = "INonReplayableProjection")]
    public class NonReplayableProjection : IProjection, INonVersionableProjection { }

    [DataContract(Name = "INonRebuildableProjection")]
    public class NonRebuildableProjection : IProjection, INonRebuildableProjection { }
}
