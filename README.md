Cronus is a lightweight framework for building event driven systems with DDD/CQRS in mind

===============
[![Build status](https://ci.appveyor.com/api/projects/status/0ka8b6vnwjj9lhav?svg=true)](https://ci.appveyor.com/project/Elders-OSS/cronus)
[![NuGet](https://img.shields.io/nuget/v/Cronus.svg)](https://www.nuget.org/packages/Cronus)
[![GitHub issues](https://img.shields.io/github/issues/Elders/Cronus/shields.svg)](https://github.com/Elders/Cronus/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr/Elders/Cronus.svg)](https://github.com/Elders/Cronus/pulls)

# Motivation

Building software is not an easy task. It involves specific domain knowledge and a lot of software infrastructure. The goal of Cronus is to keep software engineers focused on the domain problems because this is important at the end of the day. Cronus aims to keep you away from the software infrastructure.

Usually you do not need a CQRS framework to develop great apps. However, we noticed a common infrastructure code written with every application. We started to abstract and move it to Github. The key aspect being that even with a framework you must still have full control and flexibility over the application code.

# Domain Modeling

To get out the maximum of Cronus you need to mark certain parts of your code to give hints to Cronus.

## Serialization

[ISerializer](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Serializer/ISerializer.cs#L5-L9) interface is simple. You can plug your own implementation in but should not do this once you are in production.

The samples on this page work with Json and Proteus-protobuf serializers. Every ICommand, IEvent, ValueObject or anything which is persisted is marked with a DataContractAttribute and the properties are marked with a DataMemberAttribute. [Here is a quick sample how this works (just ignore the WCF or replace it with Cronus while reading)](https://msdn.microsoft.com/en-us/library/bb943471%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396). We use `Guid` for the name of the DataContract because it is unique.

#### You can/should/must...

- you must add private parameterless constructor
- you must initialize all collections in the constructor(s)
- you can rename any class whenever you like even when you are already in production
- you can rename any property whenever you like even when you are already in production
- you can add new properties

#### You must not...

- you must not delete a class when already deployed to production
- you must not remove/change the `Name` of the DataContractAttribute when already deployed to production
- you must not remove/change the `Order` of the DataMemberAttribute when deployed to production. You can change the visibility modifier from `public` to `private`

## ICommand

A command is used to dispatch domain model changes. It can be accepted or rejected depending on the domain model invariants.

| Triggered by | Description |
|:--------------:|:-------------|
| UI | It is NOT common practice to send commands directly from the UI. Usually the UI communicates with web APIs. |
| API | APIs sit in the middle between UI and Server translating web requests into commands |
| External System | It is NOT common practice to send commands directly from the External System. Usually the External System communicates with web APIs. |
| IPort | Ports are a simple way for an aggregate root to communicate with another aggregate root. |
| ISaga | Sagas are a simple way for an aggregate root to do complex communication with other aggregate roots. |

| Handled by | Description |
|:----------:|-------------|
| IAggregateRootApplicationService | This is a handler where commands are received and delivered to the addressed AggregateRoot. We call these handlers ApplicationService. This is the write side in CQRS. |

#### You can/should/must...

- a command must be immutable
- a command must clearly state a business intent with a name in imperative form
- a command can be rejected due to domain validation, error or other reason
- a command must update only one AggregateRoot

#### Example

```cs
public class DeactivateAccount : ICommand
{
    DeactivateAccount() {}
    public DeactivateAccount(AccountId id, Reason reason)
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

## IAggregateRootApplicationService

This is a handler where commands are received and delivered to the addressed AggregateRoot. We call these handlers *ApplicationService*. This is the *write side* in CQRS.

| Triggered by | Description |
|:--------------:|:-------------|
| ICommand | A command is used to dispatch domain model changes. It can either be accepted or rejected depending on the domain model invariants |

#### You can/should/must...

- an appservice can load an aggregate root from the event store
- an appservice can save new aggregate root events to the event store
- an appservice can establish calls to the ReadModel (not common practice but sometimes needed)
- an appservice can establish calls to external services
- you can do dependency orchestration
- an appservice must be stateless
- an appservice must update only one aggregate root. Yes, this means that you can create one aggregate and update another one but think twice

#### You should not...

- an appservice should not update more than one aggregate root in single command/handler
- you should not place domain logic inside an application service
- you should not use application service to send emails, push notifications etc. Use Port or Gateway instead
- an appservice should not update the ReadModel

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

## IAggregateRoot - triggered by ApplicationService

| Triggered by | Description |
|:------------:|:-------------|
| IAggregateRootApplicationService | This is a handler where commands are received and delivered to the addressed AggregateRoot. We call these handlers *ApplicationService*. This is the *write side* in CQRS. |

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

## IEvent - triggered by IAggregateRoot

Domain events represent business changes which already happened.

| Triggered by | Description |
|:------------:|:------------|
| IAggregateRoot | TODO |

#### You can/should/must...

- an event must be immutable
- an event must represent a domain event which already happened with a name in past tense
- an event can be dispatched only by one aggregate

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

## IProjection

Projection tracks events and project their data for specific purposes.

| Triggered by | Description |
|:------------:|:------------|
| IEvent | Domain events represent business changes which have already happened |

#### You can/should/must...

- a projection must be idempotent
- a projection must not issue new commands or events

#### You should not...

- a projection should not query other projections. All the data of a projection must be collected from the Events' data
- a projection should not establish calls to external systems

## IPort

Port is the mechanism to establish communication between aggregates. Usually this involves one aggregate who triggered an event and one aggregate which needs to react.

If you feel the need to do more complex interactions, it is advised to use ISaga. The reason for this is that ports do not provide a transparent view of the business flow because they do not have persistent state.

| Triggered by | Description |
|:------------:|:------------|
| IEvent | Domain events represent business changes which have already happened |

#### You can/should/must...

- a port can send a command

## ISaga/ProcessManager

When we have a workflow, which involves several aggregates it is recommended to have the whole process described in a single place such as а Saga/ProcessManager.

| Triggered by | Description |
|:------------:|:------------|
| IEvent | Domain events represent business changes which have already happened |

#### You can/should/must...

- a saga can send new commands

## IGateway

Compared to IPort, which can dispatch a command, an IGateway can do the same but it also has a persistent state. A scenario could be sending commands to external BC, such as push notifications, emails etc. There is no need to event source this state and its perfectly fine if this state is wiped. Example: iOS push notifications badge. This state should be used only for infrastructure needs and never for business cases. Compared to IProjection, which tracks events, projects their data, and are not allowed to send any commands at all, an IGateway can store and track metadata required by external systems. Furthermore, IGateways are restricted and not touched when events are replayed.

| Triggered by | Description |
|:------------:|:------------|
| IEvent | Domain events represent business changes which have already happened |

#### You can/should/must...

- a gateway can send new commands

# Ecosystem

Legend
------

| Name | Description |
|------|-------------|
| ![Elders](https://img.shields.io/badge/Status-olympus-green.svg) | It is stable and it will continue to get support, maintenance and future development |
| ![Elders](https://img.shields.io/badge/Status-styx-orange.svg) | The future is not clear. There are two possible paths from here - olympus or tartarus |
| ![Elders](https://img.shields.io/badge/Status-tartarus-red.svg) | abandoned |

Domain Modeling / Core

----------------------

| Name | Links | Status | Description |
|------|-------|--------|-------------|
| DomainModeling | [code](https://github.com/Elders/Cronus.DomainModeling) [![NuGet](https://img.shields.io/nuget/v/Cronus.DomainModeling.svg)](https://www.nuget.org/packages/Cronus.DomainModeling) [![GitHub issues](https://img.shields.io/github/issues/Elders/Cronus.DomainModeling/shields.svg)](https://github.com/Elders/Cronus.DomainModeling/issues) [![GitHub pull requests](https://img.shields.io/github/issues-pr/Elders/Cronus.DomainModeling.svg)](https://github.com/Elders/Cronus.DomainModeling/pulls) | ![Elders](https://img.shields.io/badge/Status-olympus-green.svg) | Contracts for DDD/CQRS development |
| Cronus | [code](https://github.com/Elders/Cronus) [![NuGet](https://img.shields.io/nuget/v/Cronus.svg)](https://www.nuget.org/packages/Cronus) | ![Elders](https://img.shields.io/badge/Status-olympus-green.svg) | Cronus is a lightweight framework for dispatching and receiving messages between microservices with DDD/CQRS in mind |

Messaging
---------

| Name | Status | Description |
|------|--------|-------------|
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
| [Protobuf (Proteus)](https://github.com/Elders/Cronus.Serialization.Proteus) | styx | This has been the prefered serialization with Cronus v2. However, there is a huge warm up performance hit with big projects which needs to be resolved. Despite this, it works really fast. The implementation has small protocol changes |

Projections persistence
-----------------------

| Store | Status | Description |
|------------|--------|-------------|
| [Cassandra](https://github.com/Elders/Cronus.Projections.Cassandra) | olympus | Stores projections in Cassandra |
| [ElasticSearch](https://github.com/Elders/Cronus.Projection.ElasticSearch) | olympus | Builds projections dynamically. Very useful for projects which are just starting, and changes occur frequently. However, it must be switched to another persister such as Cassandra after the initial stages of the project |
| [AtomicAction](https://github.com/Elders/Cronus.AtomicAction.Redis) | olympus | Aggregate distributed lock with Redis |

Other
-------------

| Name | Status | Description |
|------------|--------|-------------|
| [Hystrix](https://github.com/Elders/Cronus.Hystrix) | olympus | Circuit breaker middleware for Cronus |
| [Migrations](https://github.com/Elders/Cronus.Migration.Middleware) | olympus | Middleware to handle migrations of any kind |
| [AtomicAction](https://github.com/Elders/Cronus.AtomicAction.Redis) | olympus | Aggregate distributed lock with Redis |
