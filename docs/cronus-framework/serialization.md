# Serialization

 [ISerializer](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Serializer/ISerializer.cs#L5-L9) interface is simple. You can plug your own implementation in but should not change it once you are in production.

 The samples in this manual work with Json and Proteus-protobuf serializers. Every Command, Event, ValueObject or anything which is persisted is marked with a DataContractAttribute and the properties are marked with a DataMemberAttribute. [Here is a quick sample how this works \(just ignore the WCF or replace it with Cronus while reading\)](https://msdn.microsoft.com/en-us/library/bb943471%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396). We use `Guid` for the name of the DataContract because it is unique.

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **must** add private parameterless constructor
* you **must** initialize all collections in the constructor\(s\)
* you **can** rename any class whenever you like even when you are already in production
* you **can** rename any property whenever you like even when you are already in production
* you **can** add new properties
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **must not** delete a class when already deployed to production
* you **must not** remove/change the `Name` of the `DataContractAttribute` when already deployed to production
* you **must not** remove/change the `Order` of the `DataMemberAttribute` when deployed to production. You can change the visibility modifier from `public` to `private`
{% endhint %}

