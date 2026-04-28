# Unit testing

Cronus aggregates are a joy to test. An aggregate is a pure state machine — given a history of events, applying a command produces zero or more new events. No databases, no message brokers, no mocks: just a sequence of inputs and a sequence of outputs. The `Elders.Cronus.Testing` helpers shipped in [`Cronus.DomainModeling`](https://github.com/Elders/Cronus.DomainModeling) lean into that, and the pattern you end up using is always the same.

## The `Aggregate<T>.FromHistory(...)` pattern

The shipped helper is `Aggregate<T>` under the `Elders.Cronus.Testing` namespace. It hands you a fluent stream builder, replays the events through the aggregate's `When` handlers, and returns an instance that is ready to have a command executed against it.

A representative test from the Cronus test suite — [`When_projection_version_with_status_building_is_outside_of_the_timebox - Copy.cs`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus.Tests/Projections/When_projection_version_with_status_building_is_outside_of_the_timebox%20-%20Copy.cs) — reads:

```csharp
using Elders.Cronus.Testing;

ar = Aggregate<ProjectionVersionManager>
    .FromHistory(stream => stream
        .AddEvent(new ProjectionVersionRequested(id, new ProjectionVersion(...), ...))
        .AddEvent(new ProjectionVersionRequested(id, new ProjectionVersion(...), ...))
        .AddEvent(new ProjectionVersionRequestTimedout(id, new ProjectionVersion(...), ...)));

// When: execute a method on the aggregate root
ar.Replay(hash, new MarkupInterfaceProjectionVersioningPolicy(), new ReplayEventsOptions());

// Then: assert on the uncommitted events produced by the command
ar.PublishedEvents<ProjectionVersionRequestTimedout>().Count().ShouldEqual(2);
```

The three-step shape — `Given` the history, `When` the command, `Then` the published events — is the shape of every aggregate test. The `PublishedEvents<TEvent>()` helper filters the uncommitted events on the aggregate by type so you can assert on exactly the facts you expect the command to have produced.

## Why this works

The reason the test is so compact is structural: `AggregateRoot<TState>` is a reducer. Its state changes _only_ through `When(TEvent)` handlers, and the aggregate methods never do I/O — they compute the next event and call `Apply`, which runs through `When` and appends to `UncommittedEvents`. `Aggregate<T>.FromHistory` is doing in a few lines what `AggregateRepository.LoadAsync` does at runtime: it creates a fresh instance of the aggregate and feeds the history through `ReplayEvents`.

The same property makes aggregate tests uniquely valuable. Writing a failing test for a bug is usually one copy-paste away from the production event log that revealed it: the same events, in the same order, and the assertion that the next command produces the event that should have been produced.

## A small full example

Suppose you are modelling a `Concert` aggregate with an `Announce` method and a `RegisterPerformer` method. The test that a performer cannot be registered after the concert has already started:

```csharp
using Elders.Cronus.Testing;
using Machine.Specifications;

[Subject("Concert")]
public class When_registering_a_performer_after_the_concert_has_started
{
    Establish context = () =>
    {
        concertId = new ConcertId("summer-festival", "eldersoss");

        concert = Aggregate<Concert>
            .FromHistory(stream => stream
                .AddEvent(new ConcertAnnounced(concertId, "Summer Festival", venue, startTime, duration))
                .AddEvent(new ConcertStarted(concertId, startTime)));
    };

    Because of = () => registerResult = Catch.Exception(
        () => concert.RegisterPerformer(new Performer("Some Band")));

    It should_throw = () => registerResult.ShouldBeOfExactType<InvalidOperationException>();

    It should_not_publish_a_performer_registered_event =
        () => concert.PublishedEvents<PerformerRegistered>().ShouldBeEmpty();

    static ConcertId concertId;
    static Concert concert;
    static Exception registerResult;
}
```

Five lines of arrange; one line of act; two lines of assert. No fixtures, no setup, no async, no mocks — because the aggregate is a pure reducer.

## Integration-style tests

When you want to test the path _through_ the repository — integrity checks, atomic-action retries, the aggregate-commit interceptor — use the in-memory event store that Cronus wires by default in tests:

```csharp
var services = new ServiceCollection()
    .AddLogging()
    .AddCronus(...)                        // uses InMemory everything
    .BuildServiceProvider();

var repository = services.GetRequiredService<IAggregateRepository>();

await repository.SaveAsync(concert);        // exercises the real IEventStore, with the in-memory backend
var roundtripped = await repository.LoadAsync<Concert>(concertId);
```

This is slower than `Aggregate<T>.FromHistory` and you should still prefer the pure-reducer test as the first-class unit test. Reach for the integration test when the question you are answering is about the repository, not about the aggregate.

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **should** write one aggregate test per business rule, shaped as _Given events → When method → Then events_
* you **should** use the events directly from a failing production replay as your "Given" when you reproduce a bug
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **should not** mock `AggregateRepository` in an aggregate test; you are testing the reducer, not the repository
* you **must not** assert on the aggregate's private state; assert on the events, they are the only public output
{% endhint %}
