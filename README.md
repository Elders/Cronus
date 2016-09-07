Cronus is a lightweight framework for dispatching and receiving messages between microservices with DDD/CQRS in mind
==================================================================================================================
[![Build status](https://ci.appveyor.com/api/projects/status/0ka8b6vnwjj9lhav?svg=true)](https://ci.appveyor.com/project/Elders-OSS/cronus)

#Motivation
Building software is not an easy task. It involves specific domain knowledge and a lot of software infrastructure. The goal of Cronus is to keep the software engineers focused on the domain problems because this is important at the end of the day. Cronus aims to keep you away from the software infrastructure.  

Usually you do not need a CQRS framework to develop greate apps. However we noticed a common infrastructure code written with every applicaiton. We started to abstract and move that code to github. The key aspect was that even with a framework you still have full control and flexibility over the application code.

#Domain Modeling
To get out the maximum of Cronus you need to mark certain parts of your code to give hints to Cronus. 

##Serialization
[ISerializer](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Serializer/ISerializer.cs#L5-L9) interface is really simple. You can plugin your own implementation but do not do it once you are in production.

The samples bellow work with Json and Proteus-protobuf serializers. Every ICommand, IEvent, ValueObject and anything which is stored are marked with a DataContractAttribute and the properties are marked with a DataMemberAttribute. [Here is a quick sample how this works (just ignore the WCF or replace it with Cronus while reading)](https://msdn.microsoft.com/en-us/library/bb943471%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396). We use `Guid` for the name of the DataContract because it is unique.

####You can/should/must...
- you must add private parameterless constructor
- you must initialize all collections in the constructor(s)
- you can rename any class whenever you like even when you are already in production
- you can rename any property whenever you like even when you are already in production
- you can add new properties

You must not...
- you must not delete a class when already deployed to production;
- you must not remove/change the `Name` of the DataContractAttribute when already deployed to production;
- you must not remove/change the `Order` of the DataMemberAttribute when deployed to production. You can change the visibility modifier from `public` to `private`;


##ICommand
A command is used to dispatch domain model changes. It can be accepted or rejected depending on the domain model invariants.

| Triggered by | Description |
|:--------------:|:-------------|
| UI | It is NOT a common practice to send commands directly from the UI. Usually the UI communicates with web APIs. |
| API | APIs sit in the middle between UI and Server translating web requests into commands |
| External System | It is NOT a common practice to send commands directly from the External System. Usually the External System communicates with web APIs. |
| IPort | Ports are simple way for an aggregate root to communicate with another aggregate root. |
| ISaga | Sagas are complex way for an aggregate root to communicate with another aggregate root even with external bounded context. |

| Handled by | Description |
|------------|-------------|
| IAggregateRootApplicationService | This is a handler where commands are received and delivered to the addressed AggregateRoot. We call these handlers ApplicationService. This is the write side in CQRS. |

####You can/should/must...
- a command must be immutable
- a command must clearly state a business intent with a name in imperative form
- a command can be rejected due to domain validation, error or other reason
- a command must update only one AggregateRoot

####Example
```cs
public class DeactivateAccount : ICommand
{
	DeactivateAccount() {}
    public DeactivateAccount(AccountId id, string reason)
    {
    	Id = id;
    	Reason = reason;
    }

    public AccountId Id { get; private set; }
    public Reason ReasonToDeactivate { get; private set; }
}

[DataContract(Name = "24c59143-b95e-4fd6-8bbf-8d5efffe3185")]
public class AccountId : StringTenantId
{
    protected AccountId() { }
    public AccountId(string id, string tenant) : base(id, "account", tenant) { }
    public AccountId(IUrn urn) : base(urn, "account") { }
}

public class Reason : ValueObject<Reason>{...}
```


##IAggregateRootApplicationService - triggered by ICommand
This is a handler where commands are received and delivered to the addressed AggregateRoot. We call these handlers *ApplicationService*. This is the *write side* in CQRS.

####You can/should/must...
- you can load an aggregate root from the event store
- you can save new aggregate root events to the event store
- you can do calls to the ReadModel
- you can do calls to external services
- you can do dependency orchestration
- An ApplicationService must be stateless
- you must update only one aggreate root. Yes, this means that you can create one aggregate and update another one but think twice

####You should not...
- you should not update more than one aggregate root in single command/handler
- you should not place domain logic inside an application service
- you should not use application service to send emails, push notifications etc. Use Port or Gateway instead
- you should not update the ReadModel from an ApplicationService


```cs
public class AccountAppService : AggregateRootApplicationService<Account>,
    ICommandHandler<RegisterAccount>,
    ICommandHandler<ActivateAccount>,
    ICommandHandler<SuspendAccount>,
    ICommandHandler<ResetAccountPassword>,
    ICommandHandler<ChangeAccountEmail>,
    ICommandHandler<ChangeAccountUsername>
{
    public void Handle(SuspendAccount message)
    {
        Update(message.Id, account => account.Suspend());
    }
    
    ...
}
```


##IAggregateRoot - triggered by ApplicationService

```cs
public class Account : AggregateRoot<AccountState>
{
    Account() { }

    public Account(AccountId id, string username, string password, Email email)
    {
        if (ReferenceEquals(null, id)) throw new ArgumentNullException(nameof(id));
        if (ReferenceEquals(null, username)) throw new ArgumentNullException(nameof(username));
        if (ReferenceEquals(null, password)) throw new ArgumentNullException(nameof(password));

        state = new AccountState();
        var evnt = new NewAccountRegistered(id, username, password, email);
        Apply(evnt);
    }

    public void Suspend()
    {
        if (!state.IsSuspended)
        {
            var evnt = new AccountSuspended(state.Id);
            Apply(evnt);
        }
    }
    
    ...
}
```

```cs
public class AccountState : AggregateRootState<Account, AccountId>
{
    public override AccountId Id { get; set; }

    public string Username { get; set; }

    public Email Email { get; set; }

    public string Password { get; set; }

    public bool IsSuspended { get; set; }

    public void When(NewAccountRegistered e)
    {
        Id = e.Id;
        Email = e.Email;
        Password = e.Password;
    }

    public void When(AccountSuspended e)
    {
        IsSuspended = true;
    }
    
    ...
}
```

##IEvent - triggered by IAggregateRoot
Markup interface. Represents domain model changes.


An Event must be immutable
Should represent a domain event which already happened. The name of the event must be in past tense.
A Command must clearly state a business intent with a name in imperative form  
A Command can be rejected due to domain validation, error or other reason  
A Command must update only one AggregateRoot

```cs
[DataContract(Name = "fff400a3-1af0-4332-9cf5-b86c1c962a01")]
public class AccountSuspended : IEvent
{
    AccountSuspended() { }

    public AccountSuspended(AccountId id)
    {
        Id = id;
    }

    [DataMember(Order = 1)]
    public AccountId Id { get; private set; }

    public override string ToString()
    {
        return "Account was suspended";
    }
}
```


##IProjection - triggered by IEvent
- idempotent
- must not query other projections. All the data of a projection must be collected from the Events' data
- projections must not issue new commands or events


##ISaga/ProcessManager - triggered by IEvent


##IPort - triggered by IEvent
- You can send new commands


##IGateway

#Ecosystem


Legend
------

| Name | Description |
|------|-------------|
| olympus | It is stable and it will continue to get support, maintenance and future development |
| styx | The future is not clear. There are two possible paths from here - olympus or tartarus |
| tartarus | abandoned |


Domain Modeling / Core
----------------------

| Name | Links | Status | Description |
|------|------ |--------|-------------|
| DomainModeling | [code](https://github.com/Elders/Cronus.DomainModeling) [nuget](https://www.nuget.org/packages/Cronus.DomainModeling) | olympus | Contains contracts for DDD/CQRS development |
| Cronus | [code](https://github.com/Elders/Cronus) [nuget](https://www.nuget.org/packages/Cronus) | olympus | Cronus is a lightweight framework for dispatching and receiving messages between microservices with DDD/CQRS in mind |


Messaging
---------

| Broker | Status | Description |
|--------|--------|-------------|
| [RabbitMQ](https://github.com/Elders/Cronus.Transport.RabbitMQ) | olympus | It works so well that we do not need to implement other messaging. |


Event store persistence
------------------------

| Store | Status | Description |
|-------|--------|-------------|
| [Cassandra](https://github.com/Elders/Cronus.Persistence.Cassandra) | olympus | This persister is in production since 2013 and it is recommended for production purposes. |
| [MSSQL](https://github.com/Elders/Cronus.Persistence.MSSQL) | styx | The persister has been used in production with Cronus v1 but MSSQL is relational database and it does not fit well as an event store persister. |
| [GIT](https://github.com/Elders/Cronus.Persistence.Git-) | tartarus | Persister exists just for fun. |


Serialization
-------------

| Serializer | Status | Description |
|------------|--------|-------------|
| [Json](https://github.com/Elders/Cronus.Serialization.NewtonsoftJson) | olympus | It is recommended to use the serializer with DataContracts |
| [Protobuf (Proteus)](https://github.com/Elders/Cronus.Serialization.Proteus) | styx | This has been the prefered serialization with Cronus v2. However, there is a huge warm up performance hit with big projects which needs to be resolved. Despite this it works really fast. The implementation has small protocol changes |


Projections persistence
-----------------------

| Store | Status | Description |
|------------|--------|-------------|
| [Cassandra](https://github.com/Elders/Cronus.Projections.Cassandra) | olympus | Stores projections in Cassandra |
| [ElasticSearch](https://github.com/Elders/Cronus.Projection.ElasticSearch) | olympus | Builds projections dynamically. Very usefull for projects which just started and changes occur frequently. Later must be switch to other persister such as Cassandra |
| [AtomicAction](https://github.com/Elders/Cronus.AtomicAction.Redis) | olympus | Aggregate distrubited lock with Redis |

Other
-------------

| Name | Status | Description |
|------------|--------|-------------|
| [Hystrix](https://github.com/Elders/Cronus.Hystrix) | olympus | Circuit breaker middleware for Cronus |
| [Migrations](https://github.com/Elders/Cronus.Migration.Middleware) | olympus | Middleware to handle migrations of any kind |
| [AtomicAction](https://github.com/Elders/Cronus.AtomicAction.Redis) | olympus | Aggregate distrubited lock with Redis |

