# Commands

A command is a simple immutable object that is sent to the domain to trigger a state change. There should be a single command handler for each command. It is recommended to use imperative verbs when naming commands together with the name of the aggregate they operate on.

{% page-ref page="../handlers/application-services.md" %}

It is possible for a command to get rejected if the data it holds is incorrect or inconsistent with the current state of the aggregate.

{% hint style="success" %}
**You can/should/must...**

* a command **must** be immutable
* a command **should** clearly state a business intent with a name in the imperative form
* a command **can** be rejected due to domain validation, error or other reason
* a command **must** update only one aggregate
{% endhint %}

## Defining a command

You can define a command with Cronus using the `ICommand` markup interface. All commands get serialized and deserialized, that's why you need to keep the parameterless constructor and specify data contracts.

{% page-ref page="../../messaging/serialization.md" %}

```csharp
// TODO: give relevant example
[DataContract(Name = "13d0f763-0cb1-43ae-af03-614680c3575a")]
public class CreateExample : ICommand
{
    CreateExample() {}
    
    public CreateExample(ExampleId id, ExampleName name)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        if (name is null) throw new ArgumentNullException(nameof(name));

        Id = id;
        Name = name;
    }

    [DataMember(Order = 1)]
    public ExampleId Id { get; private set; }

    [DataMember(Order = 2)]
    public ExampleName Name { get; private set; }

    public override string ToString()
    {
        return $"Create an example with id '{Id}' and name '{Name}'";
    }
}
```

{% hint style="info" %}
Cronus uses the `ToString()` method for logging, so you can override it to generate user-readable logs. Otherwise, the name of the command class will be used for log messages. 
{% endhint %}

## Publishing a command

To publish a command, inject an instance of `IPublisher<ICommand>` into your code and invoke the `Publish()` method passing the command. This method will return `true` if the command has been published successfully through the configured transport. You can also use one of the overrides of the `Publish()` method to delay or schedule a command.

```csharp
// TODO: give relevant example
public class CreateExampleController : ControllerBase
{
    private IPublisher<ICommand> publisher;
    
    public GetExampleController(IPublisher<ICommand> publisher)
    {
        this.publisher= publisher;
    }

    public async Task<IActionResult> CreateExample(CreateExampleRequest request)
    {
				var id = ExampleId.New(request.Tenant, request.Id);
				var name = new ExampleName(request.Name);
				var command = new CreateExample(id, name);
        var published = publisher.Publish(command);
        if (published)
            return Accepted(id);
        else
            return StatusCode(406, "Unable to process the request");
    }
}
```

