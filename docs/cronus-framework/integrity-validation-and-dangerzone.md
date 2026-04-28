# Integrity validation

When Cronus loads an aggregate from the event store, it does not blindly fold every event it finds into the state. The stream is first run through an **integrity policy** that checks the events make a coherent history — no duplicates, no out-of-order revisions, no holes. If the policy reports a violation that no resolver could fix, the load fails. This page covers the policy extension point, the default rules, and the (deliberate) absence of a "skip the policy" bypass in the shipped framework.

## What integrity validation does

Every load goes through [`AggregateRepository.LoadAsync`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/AggregateRepository.cs):

```csharp
public async Task<ReadResult<AR>> LoadAsync<AR>(AggregateRootId id) where AR : IAggregateRoot
{
    EventStream eventStream = await eventStore.LoadAsync(id).ConfigureAwait(false);
    var integrityResult = integrityPolicy.Apply(eventStream);
    if (integrityResult.IsIntegrityViolated)
        throw new EventStreamIntegrityViolationException($"AR integrity is violated for ID={id.Value}");
    eventStream = integrityResult.Output;
    // ... fold into aggregate state ...
}
```

The repository asks the registered [`IIntegrityPolicy<EventStream>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/IntegrityValidation/IIntegrityPolicy.cs) to inspect the freshly loaded `EventStream`. If the policy returns `IsIntegrityViolated = true`, the load throws and the application service does not see a half-broken aggregate. If the policy is satisfied, the (possibly rewritten) stream is folded into the state.

The policy in core is [`EventStreamIntegrityPolicy`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Integrity/EventStreamIntegrityPolicy.cs). It is composed of three rules applied in order:

1. `DuplicateRevisionsValidator` — fails if two commits in the stream share a revision number.
2. `OrderedRevisionsValidator` paired with `UnorderedRevisionsResolver` — flags out-of-order revisions and lets the resolver attempt to put them back in order.
3. `MissingRevisionsValidator` — fails if a revision number is missing from the sequence.

Each rule is an [`IntegrityRule<EventStream>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/IntegrityValidation/IntegrityRule.cs) — a pair of an [`IValidator<T>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/IntegrityValidation/IValidator.cs) (does the stream look right?) and an [`IResolver<T>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/IntegrityValidation/IResolver.cs) (if it does not, can we recover?). The default resolver is [`EmptyResolver<T>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/IntegrityValidation/IResolver.cs), which always reports the stream as violated — no recovery, raise the exception.

## The IIntegrityPolicy extension point

The interface is small:

```csharp
public interface IIntegrityPolicy<T>
{
    IEnumerable<IntegrityRule<T>> Rules { get; }

    IntegrityResult<T> Apply(T candidate);
}
```

A policy returns a sealed [`IntegrityResult<T>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/IntegrityValidation/IntegrityResult.cs):

```csharp
public sealed class IntegrityResult<T>
{
    public IntegrityResult(T output, bool isIntegrityViolated) { /* ... */ }
    public bool IsIntegrityViolated { get; }
    public T Output { get; }
}
```

`Output` is the (possibly rewritten) candidate. `IsIntegrityViolated` is the verdict.

Validators implement [`IValidator<T>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/IntegrityValidation/IValidator.cs):

```csharp
public interface IValidator<T> : IComparable<IValidator<T>>
{
    IValidatorResult Validate(T candidate);
    uint PriorityLevel { get; }
}
```

[`IValidatorResult`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/IntegrityValidation/IValidatorResult.cs) carries `IsValid`, an `ErrorType` discriminator and an `Errors` collection. The default concrete result is [`ValidatorResult`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/IntegrityValidation/ValidatorResult.cs) — a list of error strings plus the type tag.

Resolvers implement [`IResolver<T>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/IntegrityValidation/IResolver.cs):

```csharp
public interface IResolver<T> : IComparable<IResolver<T>>
{
    IntegrityResult<T> Resolve(T eventStream, IValidatorResult validatorResult);
    uint PriorityLevel { get; }
}
```

A resolver is given the offending stream and the validator's report. It returns either a recovered `IntegrityResult<T>` with `IsIntegrityViolated = false` and a corrected `Output`, or it gives up and returns `IsIntegrityViolated = true`.

## Replacing the policy

The repository takes its `IIntegrityPolicy<EventStream>` from the container, so a satellite — or your own discovery — can replace it with a stricter or more permissive variant. Use a [discovery](extensibility/discoveries.md) with `CanOverrideDefaults = true`:

```csharp
public class CustomIntegrityPolicyDiscovery : DiscoveryBase<IIntegrityPolicy<EventStream>>
{
    protected override DiscoveryResult<IIntegrityPolicy<EventStream>> DiscoverFromAssemblies(DiscoveryContext context)
    {
        var policyModel = new DiscoveredModel(
            typeof(IIntegrityPolicy<EventStream>),
            typeof(MyStrictEventStreamIntegrityPolicy),
            ServiceLifetime.Singleton);
        policyModel.CanOverrideDefaults = true;

        return new DiscoveryResult<IIntegrityPolicy<EventStream>>([policyModel]);
    }
}
```

The same shape lets you write a permissive policy that knows how to repair a specific corruption pattern that turned up once in production, by adding a custom resolver to the list. Keep that policy package gated to the bounded context that needs it.

## Worked example: an unordered-revisions resolver

Suppose a Cassandra topology change once produced streams where revisions are correct but the rows came back in the wrong order. The default `OrderedRevisionsValidator` flags this; the default resolver (in core, before [`EventStreamIntegrityPolicy`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Integrity/EventStreamIntegrityPolicy.cs) wires `UnorderedRevisionsResolver`) would refuse the load. A resolver that sorts the commits in place and returns a healthy result fixes the case without dropping data:

```csharp
public sealed class SortByRevisionResolver : IResolver<EventStream>
{
    public uint PriorityLevel => 100;

    public int CompareTo(IResolver<EventStream> other) => PriorityLevel.CompareTo(other.PriorityLevel);

    public IntegrityResult<EventStream> Resolve(EventStream eventStream, IValidatorResult validatorResult)
    {
        var sorted = eventStream.Commits.OrderBy(c => c.Revision).ToList();
        var fixedStream = new EventStream(sorted);
        return new IntegrityResult<EventStream>(fixedStream, isIntegrityViolated: false);
    }
}
```

Pair it with the validator, register the rule on a custom policy, override the default through a discovery, and the next load that hits the bad pattern recovers without a manual repair.

## "DangerZone" — what is and is not in the framework

Some Cronus discussions refer to a **DangerZone** — a deliberate, ugly bypass of the integrity policy for one-off corrective actions. **There is no `DangerZone` namespace, no `IDangerZone` interface, and no `Cronus:DangerZone:*` configuration key in the shipped framework today**. Every load goes through the registered `IIntegrityPolicy<EventStream>`, and the only way to "bypass" the standard checks is to register a more permissive policy via a discovery — which is itself the first-class extension point.

If you find code or scripts that mention DangerZone, treat them as out-of-band repair tooling that lives outside the Cronus host, not as a documented framework feature. The expected pattern for a corrective action is:

1. Stop the host (or the relevant consumers).
2. Apply the repair directly against the event store, in a one-shot tool that you can audit.
3. Restart the host.

Do not try to encode the bypass as a permanent flag.

## Best Practices

{% hint style="success" %}
**You can / should / must**

* an integrity policy **must** treat a violation as a failure unless a resolver can prove the stream is recoverable
* a custom validator **should** populate `ErrorType` with a stable string so log searches can find every occurrence of the same defect
* a custom resolver **should** preserve every commit it can — repair, do not drop
* a stricter policy **can** be shipped as a satellite discovery and applied to a single bounded context where the constraint matters most
{% endhint %}

{% hint style="warning" %}
**You should not**

* a policy **should not** be replaced with a no-op "always succeed" implementation in production — the integrity check exists because the alternative is a silently corrupted aggregate
* a resolver **should not** mutate the input stream in place; build a new `EventStream` and return it via `IntegrityResult<EventStream>`
* an integrity violation **should not** be swallowed in the application-service layer; let `EventStreamIntegrityViolationException` surface to the caller and the operator
{% endhint %}

{% hint style="danger" %}
**Repair is a one-off action, not a feature flag.** Even when there is a real, justified reason to load a stream that the policy refuses, do it from a separate one-shot tool that you can audit, with the host stopped, and put the policy back in place when you are done. There is no shipped flag that says "skip integrity validation for the next N loads", and that is on purpose.
{% endhint %}
