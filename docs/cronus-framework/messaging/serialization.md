# Serialization

Every message that lives longer than a single process call — an event in the store, a command on the wire, a public event crossing a bounded context — goes through a serializer on the way out and through the same serializer on the way in. Serialization is the contract between "now" and "every version of the service that will ever run".

## The contract

The interface in [`Cronus/src/Elders.Cronus/ISerializer.cs`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/ISerializer.cs) is deliberately minimal:

```csharp
public interface ISerializer
{
    byte[] SerializeToBytes<T>(T message);
    string SerializeToString<T>(T message);
    T DeserializeFromBytes<T>(byte[] bytes);
}
```

The implementation you wire into DI is the one the entire framework uses — there is no per-message switch. You should not change it once a service is in production without doing a full event-store migration, because every byte stored before the switch was produced by the previous serializer.

## The contract-id convention

Every `IMessage` (every `ICommand`, `IEvent`, `IPublicEvent`, `ISignal`) and every value-typed record/class that persists bytes is annotated with a `DataContractAttribute` whose `Name` is a GUID:

```csharp
[DataContract(Name = "f69daa12-171c-43a1-b049-be8a93ff137f")]
public class AggregateCommit : IMessage { ... }
```

That guid is the _contract id_ — the stable identifier the serializer writes alongside the payload. It is the id the system uses to look up the type when deserialising:

* `GetContractId()` on a `Type` returns the guid.
* `GetTypeByContract(string contractId)` goes the other way.
* Serialized messages carry their contract id so the runtime can pick the right type to materialise into.

Because the contract id is just a guid in an attribute, you can freely rename the C# class, the namespace, the fields (as long as `[DataMember(Order = N)]` is stable), or move it between assemblies, and the persisted bytes keep working. That is the property that makes long-lived event stores maintainable — the wire shape and the code shape are allowed to diverge.

The rules that govern contract evolution are explained in full under [Published Language](../domain-modeling/published-language.md); the hint block at the bottom of this page summarises them.

## The shipped serializers

Two serializers live in the ecosystem:

* [`Cronus.Serialization.NewtonsoftJson`](https://github.com/Elders/Cronus.Serialization.NewtonsoftJson) — the canonical serializer, marked `olympus`. JSON, driven by `[DataContract]` and `[DataMember]` attributes. Human-readable bytes in the store (very helpful for debugging), broad compatibility, and zero warm-up cost. This is the one you should use.
* [`Cronus.Serialization.Proteus`](https://github.com/Elders/Cronus.Serialization.Proteus) (legacy) — the protobuf-based serializer, marked `styx`. Faster once warm, more compact on disk — but it pays a significant warm-up penalty on large projects (the type graph is walked once on first use) and the implementation has a small protocol deviation from stock protobuf. The recommendation today is the JSON serializer.

## Rules of thumb

The pattern that keeps serialization safe long-term is exactly the pattern `DataContract` encodes:

1. Each message type gets a `DataContractAttribute` with `Name` set to a new GUID. Never reuse guids.
2. Each persisted field gets `[DataMember(Order = N)]` where `N` is unique within the type and never changes.
3. Every type you persist has a private parameterless constructor (the serializer needs to build the instance before it fills it).
4. Collection fields are initialised in the constructor (otherwise a freshly-deserialised instance may expose a null list).

The best-practices block below is the full `can / should / must` form.

## Related pages

{% content-ref url="../domain-modeling/published-language.md" %}
[published-language.md](../domain-modeling/published-language.md)
{% endcontent-ref %}

{% content-ref url="README.md" %}
[README.md](README.md)
{% endcontent-ref %}

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **must** add a private parameterless constructor on every persisted type
* you **must** initialise all collection members in the constructor(s)
* you **can** rename any class whenever you like even when you are already in production
* you **can** rename any property whenever you like even when you are already in production
* you **can** add new properties — on deserialisation they get the default value
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **must not** delete a class when already deployed to production — the store still references its contract id
* you **must not** remove or change the `Name` of the `DataContractAttribute` on a deployed type
* you **must not** remove or change the `Order` of a `DataMemberAttribute` on a deployed type; you may change visibility (`public` → `private`) but never the number
{% endhint %}
