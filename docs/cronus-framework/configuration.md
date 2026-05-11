# Configuration

## Overview

Cronus and its satellite packages are configured through the standard ASP.NET Core configuration system. Every options class binds to a well-known section of `IConfiguration` under the root key `cronus:`, which means you can supply values from `appsettings.json`, environment variables, Consul/Vault providers, command-line arguments, or any other `IConfigurationProvider` you register.

Defaults are chosen so that a local development environment works out of the box. In production you almost always override at least:

* `cronus:boundedcontext` — the name of your service
* `cronus:tenants` — the list of tenants the service serves
* The connection strings for Cassandra, RabbitMQ and Redis

Configuration keys are case-insensitive. The headings below use the exact casing emitted by the option providers as a reference.

## Cronus

The core Cronus package exposes two required settings (the bounded context name and the tenant list) plus a family of boolean feature flags controlled through [`CronusHostOptions`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/CronusHostOptions.cs). The flags decide which consumers and background services Cronus starts inside the host process, so you can split a monolith into several specialised processes by toggling them.

| Name                                                                                    | Type       | Required | Default Value |
| --------------------------------------------------------------------------------------- | ---------- | -------- | ------------- |
| [Cronus:BoundedContext](configuration.md#cronus-boundedcontext)                         | string     | yes      |               |
| [Cronus:Tenants](configuration.md#cronus-tenants)                                       | string\[]  | yes      |               |
| [Cronus:Heartbeat:IntervalInSeconds](configuration.md#cronus-heartbeat-intervalinseconds) | uint     | no       | 5             |
| [Cronus:ApplicationServicesEnabled](configuration.md#cronus-applicationservicesenabled) | bool       | no       | true          |
| [Cronus:ProjectionsEnabled](configuration.md#cronus-projectionsenabled)                 | bool       | no       | true          |
| [Cronus:PortsEnabled](configuration.md#cronus-portsenabled)                             | bool       | no       | true          |
| [Cronus:SagasEnabled](configuration.md#cronus-sagasenabled)                             | bool       | no       | true          |
| [Cronus:GatewaysEnabled](configuration.md#cronus-gatewaysenabled)                       | bool       | no       | true          |
| [Cronus:TriggersEnabled](configuration.md#cronus-triggersenabled)                       | bool       | no       | true          |
| [Cronus:SystemServicesEnabled](configuration.md#cronus-systemservicesenabled)           | bool       | no       | true          |
| [Cronus:MigrationsEnabled](configuration.md#cronus-migrationsenabled)                   | bool       | no       | false         |
| [Cronus:RpcApiEnabled](configuration.md#cronus-rpcapienabled)                           | bool       | no       | false         |

#### Cronus:BoundedContext

The logical name of the service, bound from [`BoundedContext`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/BoundedContext.cs). It is marked `[Required]` and validated against the regular expression `^\b([\w\d_]+$)`, so it may contain only alphanumeric characters and underscores. The value is lower-cased and trimmed by `BoundedContextProvider`.

Cronus uses this value to personalise the infrastructure it creates on your behalf. It is used when naming:

* RabbitMQ exchanges and queues
* The Cassandra event-store keyspace and tables
* The Cassandra projections-store keyspace and tables

{% code title="appsettings.json" %}
```json
{
  "cronus": {
    "boundedcontext": "billing"
  }
}
```
{% endcode %}

#### Cronus:Tenants

The list of tenants the service is allowed to serve, bound from [`TenantsOptions`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Multitenancy/TenantsOptions.cs). The array is marked `[Required]`; every element is validated against `^\b([\w\d_]+$)` (alphanumeric plus underscore), lower-cased, trimmed and deduplicated by `TenantsOptionsProvider` before being used.

Cronus is multitenant by design and will refuse to start if the list is empty. Many components of the framework are tenant-aware, including:

* Every `CronusMessage` is tagged with the tenant it belongs to
* RabbitMQ exchanges and queues are partitioned per tenant
* The Cassandra event store keeps a dedicated keyspace per tenant
* The Cassandra projection store keeps a dedicated keyspace per tenant

Once set, the `TenantsOptions` instance is available via dependency injection wherever you need to enumerate the tenants at runtime. See [Multitenancy](domain-modeling/multitenancy.md) for the full model.

{% code title="appsettings.json" %}
```json
{
  "cronus": {
    "tenants": [ "tenant1", "tenant2", "tenant3" ]
  }
}
```
{% endcode %}

#### Cronus:Heartbeat:IntervalInSeconds

The interval, in seconds, between the signal messages Cronus emits to prove the host is alive. Bound from [`HeartbeatOptions`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/Heartbeat/HeartbeatOptions.cs). Validated with `[Range(5, 3600)]`.

{% code title="appsettings.json" %}
```json
{
  "cronus": {
    "heartbeat": {
      "intervalinseconds": 5
    }
  }
}
```
{% endcode %}

#### Cronus:ApplicationServicesEnabled

Starts the consumer that dispatches commands to application services (command handlers). Turn it off on processes that are not supposed to handle commands — for example a read-only projections host.

#### Cronus:ProjectionsEnabled

Starts the consumer that writes events into the projection store. Turn it off on processes that only dispatch commands or act as API gateways, or when you want to pause projections while running a migration.

#### Cronus:PortsEnabled

Starts the consumer that dispatches events to ports (outbound integrations such as sending e-mail or calling a third-party API). Disable it where you do not want outbound side effects, typically on read-only replicas.

#### Cronus:SagasEnabled

Starts the consumer that dispatches events to sagas (long-running processes). Disable it on hosts that must not advance saga state.

#### Cronus:GatewaysEnabled

Starts the consumer that dispatches events to gateways (components that publish to a second transport, typically the public RabbitMQ bus). Disable it on hosts that should not re-publish events outside the service.

#### Cronus:TriggersEnabled

Starts the consumer that dispatches events to triggers (hooks that react to specific events). Disable it on hosts where triggers should not fire.

#### Cronus:SystemServicesEnabled

Starts Cronus-internal system services such as the heartbeat publisher. Leave it on unless you are deliberately running a worker that must emit no system signals.

#### Cronus:MigrationsEnabled

Starts the replay/migration consumer. Defaults to `false` because running migrations in the same process that serves production traffic is usually undesirable; enable it on a dedicated migrator host while a migration is in progress.

#### Cronus:RpcApiEnabled

Starts the RPC API host that exposes request/response endpoints over RabbitMQ (see [`RpcApiDiscovery`](https://github.com/Elders/Cronus.Transport.RabbitMQ/blob/master/src/Elders.Cronus.Transport.RabbitMQ/RpcAPI/RpcApiDiscovery.cs)). Defaults to `false`; enable it on the process that should answer RPC calls.

## Cronus.Api

`Cronus.Api` hosts an ASP.NET Core endpoint with Kestrel. Two configuration sections are read directly by the host at startup:

| Name                                                            | Type                  | Required | Default Value |
| --------------------------------------------------------------- | --------------------- | -------- | ------------- |
| [Cronus:Api:Kestrel](configuration.md#cronus-api-kestrel)       | configuration section | no       |               |
| [Cronus:Api:JwtAuthentication](configuration.md#cronus-api-jwtauthentication) | configuration section | no       |               |

### Cronus:Api:Kestrel

The API is hosted with Kestrel on port `7477` by default. The whole section is forwarded to the [standard Kestrel options](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/options), so any endpoint, certificate or HTTP-protocol setting Kestrel understands is valid here.

{% code title="appsettings.json" %}
```json
{
  "Cronus": {
    "Api": {
      "Kestrel": {
        "Endpoints": {
          "Https": {
            "Url": "https://*:7477",
            "Certificate": {
              "Subject": "*.example.com",
              "Store": "My",
              "Location": "CurrentUser",
              "AllowInvalid": "true"
            }
          }
        }
      }
    }
  }
}
```
{% endcode %}

### Cronus:Api:JwtAuthentication

The API can be protected with JWT bearer authentication. If this section exists in configuration, `Cronus.Api.Startup` enables authentication and forwards the section to [`JwtBearerOptions`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.jwtbearer.jwtbeareroptions). If the section is absent, the API runs without authentication.

{% code title="appsettings.json" %}
```json
{
  "Cronus": {
    "Api": {
      "JwtAuthentication": {
        "Authority": "https://example.com",
        "Audience": "https://example.com/resources"
      }
    }
  }
}
```
{% endcode %}

Additional remarks: [https://stackoverflow.com/a/58736850/224667](https://stackoverflow.com/a/58736850/224667).

## Cronus.Persistence.Cassandra

Configuration for the Cassandra-backed event store, bound from [`CassandraProviderOptions`](https://github.com/Elders/Cronus.Persistence.Cassandra/blob/master/src/Elders.Cronus.Persistence.Cassandra/CassandraProviderOptions.cs).

| Name                                                                                                                  | Type      | Required | Default Value |
| --------------------------------------------------------------------------------------------------------------------- | --------- | -------- | ------------- |
| [Cronus:Persistence:Cassandra:ConnectionString](configuration.md#cronus-persistence-cassandra-connectionstring)       | string    | yes      |               |
| [Cronus:Persistence:Cassandra:ReplicationStrategy](configuration.md#cronus-persistence-cassandra-replicationstrategy) | string    | no       | simple        |
| [Cronus:Persistence:Cassandra:ReplicationFactor](configuration.md#cronus-persistence-cassandra-replicationfactor)     | int       | no       | 1             |
| [Cronus:Persistence:Cassandra:Datacenters](configuration.md#cronus-persistence-cassandra-datacenters)                 | string\[] | no       | \[]           |

#### Cronus:Persistence:Cassandra:ConnectionString

The connection string to the Cassandra cluster that holds the event store keyspaces.

#### Cronus:Persistence:Cassandra:ReplicationStrategy

The replication strategy used when Cronus creates the event-store keyspace for a new tenant. This setting only has an effect when a keyspace is first provisioned.

Valid values:

* `simple`
* `network_topology` — when used you must also provide `ReplicationFactor` and `Datacenters`.

#### Cronus:Persistence:Cassandra:ReplicationFactor

The replication factor used together with `ReplicationStrategy`. Only used at keyspace creation.

#### Cronus:Persistence:Cassandra:Datacenters

The datacenter names used with the `network_topology` strategy. Unused for `simple`.

{% code title="appsettings.json" %}
```json
{
  "cronus": {
    "persistence": {
      "cassandra": {
        "connectionstring": "Contact Points=cassandra;Default Keyspace=billing",
        "replicationstrategy": "network_topology",
        "replicationfactor": 3,
        "datacenters": [ "dc1", "dc2" ]
      }
    }
  }
}
```
{% endcode %}

## Cronus.Projections.Cassandra

Configuration for the Cassandra-backed projection store. The main connection settings come from [`CassandraProviderOptions`](https://github.com/Elders/Cronus.Projections.Cassandra/blob/master/src/Elders.Cronus.Projections.Cassandra/Infrastructure/CassandraProviderOptions.cs); a dedicated `TableRetention` sub-section controls how Cronus prunes old projection tables during a replay.

| Name                                                                                                                                                                                 | Type      | Required | Default Value |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------- | -------- | ------------- |
| [Cronus:Projections:Cassandra:ConnectionString](configuration.md#cronus-projections-cassandra-connectionstring)                                                                      | string    | yes      |               |
| [Cronus:Projections:Cassandra:ReplicationStrategy](configuration.md#cronus-projections-cassandra-replicationstrategy)                                                                | string    | no       | simple        |
| [Cronus:Projections:Cassandra:ReplicationFactor](configuration.md#cronus-projections-cassandra-replicationfactor)                                                                    | int       | no       | 1             |
| [Cronus:Projections:Cassandra:Datacenters](configuration.md#cronus-projections-cassandra-datacenters)                                                                                | string\[] | no       | \[]           |
| [Cronus:Projections:Cassandra:TableRetention:DeleteOldProjectionTables](configuration.md#cronus-projections-cassandra-tableretention-deleteoldprojectiontables)                      | bool      | no       | false         |
| [Cronus:Projections:Cassandra:TableRetention:NumberOfOldProjectionTablesToRetain](configuration.md#cronus-projections-cassandra-tableretention-numberofoldprojectiontablestoretain)  | uint      | no       | 2             |

#### Cronus:Projections:Cassandra:ConnectionString

The connection string to the Cassandra cluster that holds the projection keyspaces.

#### Cronus:Projections:Cassandra:ReplicationStrategy

The replication strategy used when Cronus creates the projections keyspace for a new tenant. Only effective at keyspace creation.

Valid values:

* `simple`
* `network_topology` — when used you must also provide `ReplicationFactor` and `Datacenters`.

#### Cronus:Projections:Cassandra:ReplicationFactor

The replication factor used with `ReplicationStrategy`. Only used at keyspace creation.

#### Cronus:Projections:Cassandra:Datacenters

The datacenter names used with the `network_topology` strategy. Unused for `simple`.

#### Cronus:Projections:Cassandra:TableRetention:DeleteOldProjectionTables

When a projection is rebuilt (replayed), Cronus creates a fresh table and, once the replay finishes, can optionally drop the older versions. This flag toggles that behaviour. The default is `false` — old tables are kept forever unless you opt in. The feature is experimental and was originally introduced for Cosmos DB, where every table has a direct cost.

#### Cronus:Projections:Cassandra:TableRetention:NumberOfOldProjectionTablesToRetain

When `DeleteOldProjectionTables` is enabled, this value controls how many previous versions of a projection table are kept around before older ones are deleted. The current (live) table is never counted against this limit; a value of `2` therefore means "the live table plus the two most recent historical tables".

{% code title="appsettings.json" %}
```json
{
  "cronus": {
    "projections": {
      "cassandra": {
        "connectionstring": "Contact Points=cassandra;Default Keyspace=billing_projections",
        "replicationstrategy": "simple",
        "replicationfactor": 1,
        "tableretention": {
          "deleteoldprojectiontables": false,
          "numberofoldprojectiontablestoretain": 2
        }
      }
    }
  }
}
```
{% endcode %}

## Cronus.Transport.RabbitMQ

Configuration for the primary (private) RabbitMQ connection, bound from [`RabbitMqOptions`](https://github.com/Elders/Cronus.Transport.RabbitMQ/blob/master/src/Elders.Cronus.Transport.RabbitMQ/RabbitMqOptions.cs) under the `cronus:transport:rabbitmq` section. This is the bus Cronus uses to route commands, events and signals between the services that share the same trust boundary.

| Name                                                                                                    | Type                  | Required | Default Value |
| ------------------------------------------------------------------------------------------------------- | --------------------- | -------- | ------------- |
| [Cronus:Transport:RabbitMQ:Server](configuration.md#cronus-transport-rabbitmq-server)                   | string                | no       | 127.0.0.1     |
| [Cronus:Transport:RabbitMQ:Port](configuration.md#cronus-transport-rabbitmq-port)                       | int                   | no       | 5672          |
| [Cronus:Transport:RabbitMQ:VHost](configuration.md#cronus-transport-rabbitmq-vhost)                     | string                | no       | /             |
| [Cronus:Transport:RabbitMQ:Username](configuration.md#cronus-transport-rabbitmq-username)               | string                | no       | guest         |
| [Cronus:Transport:RabbitMQ:Password](configuration.md#cronus-transport-rabbitmq-password)               | string                | no       | guest         |
| [Cronus:Transport:RabbitMQ:AdminPort](configuration.md#cronus-transport-rabbitmq-adminport)             | int                   | no       | 5672          |
| [Cronus:Transport:RabbitMQ:ApiAddress](configuration.md#cronus-transport-rabbitmq-apiaddress)           | string                | no       |               |
| [Cronus:Transport:RabbitMQ:BoundedContext](configuration.md#cronus-transport-rabbitmq-boundedcontext)   | string                | no       | implicit      |
| [Cronus:Transport:RabbitMQ:UseAsyncDispatcher](configuration.md#cronus-transport-rabbitmq-useasyncdispatcher) | bool            | no       | true          |
| [Cronus:Transport:RabbitMQ:FederatedExchange](configuration.md#cronus-transport-rabbitmq-federatedexchange) | configuration section | no   |               |
| [Cronus:Transport:RabbitMQ:ExternalServices](configuration.md#cronus-transport-rabbitmq-externalservices)   | object\[]             | no   | \[]           |

#### Cronus:Transport:RabbitMQ:Server

DNS name or IP address of the RabbitMQ broker. Multiple endpoints can be provided as a comma-separated list; they are parsed with `AmqpTcpEndpoint.ParseMultiple`.

#### Cronus:Transport:RabbitMQ:Port

The AMQP port of the RabbitMQ broker.

#### Cronus:Transport:RabbitMQ:VHost

The virtual host to use. It is good practice not to use the default `/` vhost; see the [RabbitMQ virtual hosts documentation](https://www.rabbitmq.com/vhosts.html). Cronus does not use vhosts to implement multitenancy.

#### Cronus:Transport:RabbitMQ:Username

Username used by the AMQP connection.

#### Cronus:Transport:RabbitMQ:Password

Password used by the AMQP connection.

#### Cronus:Transport:RabbitMQ:AdminPort

The HTTP port of the RabbitMQ management API. Cronus uses this port together with `ApiAddress` to create and delete exchanges, queues, policies and federation links.

#### Cronus:Transport:RabbitMQ:ApiAddress

Base URL of the RabbitMQ management API (for example `http://rabbitmq:15672`). When not set Cronus falls back to `Server` and `AdminPort`.

#### Cronus:Transport:RabbitMQ:BoundedContext

The bounded context this particular RabbitMQ connection belongs to. Defaults to `implicit`, which means "this service's own bounded context". You only need to override it when declaring an entry inside `ExternalServices` that points at another bounded context.

#### Cronus:Transport:RabbitMQ:UseAsyncDispatcher

Whether the RabbitMQ client uses the async dispatcher. Leave it at the default (`true`) unless you have a specific reason to fall back to the synchronous dispatcher.

#### Cronus:Transport:RabbitMQ:FederatedExchange

Settings for a RabbitMQ federated exchange. Currently only one property is supported:

* `MaxHops` (`int`, default `1`) — the maximum hops a federated message is allowed to travel.

{% code title="appsettings.json" %}
```json
{
  "cronus": {
    "transport": {
      "rabbitmq": {
        "federatedexchange": {
          "maxhops": 1
        }
      }
    }
  }
}
```
{% endcode %}

#### Cronus:Transport:RabbitMQ:ExternalServices

An optional array of additional RabbitMQ connections, one per external bounded context that this service needs to reach over its own transport. Each item has the same shape as the root `RabbitMqOptions` (including `BoundedContext`). If a property is left at its default value on an entry, Cronus fills it in with the value from the root section. `ExternalServices` is bound with `BindNonPublicProperties = true` because the property itself is internal to the options class.

{% code title="appsettings.json" %}
```json
{
  "cronus": {
    "transport": {
      "rabbitmq": {
        "server": "rabbitmq.internal",
        "username": "service",
        "password": "secret",
        "externalservices": [
          {
            "boundedcontext": "identity",
            "server": "rabbitmq.identity.internal"
          }
        ]
      }
    }
  }
}
```
{% endcode %}

## Cronus.Transport.RabbitMQ — Public RabbitMQ

A second, independent RabbitMQ connection used by the public-event fan-out. It is bound from [`PublicRabbitMqOptions`](https://github.com/Elders/Cronus.Transport.RabbitMQ/blob/master/src/Elders.Cronus.Transport.RabbitMQ/PublicRabbitMqOptions.cs) under the `cronus:transport:publicrabbitmq` section. Use it when you want to publish a subset of events onto a broker that lives outside your trust boundary, leaving the private broker untouched.

| Name                                                                                                                | Type                  | Required | Default Value |
| ------------------------------------------------------------------------------------------------------------------- | --------------------- | -------- | ------------- |
| [Cronus:Transport:PublicRabbitMQ:Server](configuration.md#cronus-transport-publicrabbitmq-server)                   | string                | no       | 127.0.0.1     |
| [Cronus:Transport:PublicRabbitMQ:Port](configuration.md#cronus-transport-publicrabbitmq-port)                       | int                   | no       | 5672          |
| [Cronus:Transport:PublicRabbitMQ:VHost](configuration.md#cronus-transport-publicrabbitmq-vhost)                     | string                | no       | /             |
| [Cronus:Transport:PublicRabbitMQ:Username](configuration.md#cronus-transport-publicrabbitmq-username)               | string                | no       | guest         |
| [Cronus:Transport:PublicRabbitMQ:Password](configuration.md#cronus-transport-publicrabbitmq-password)               | string                | no       | guest         |
| [Cronus:Transport:PublicRabbitMQ:AdminPort](configuration.md#cronus-transport-publicrabbitmq-adminport)             | int                   | no       | 5672          |
| [Cronus:Transport:PublicRabbitMQ:ApiAddress](configuration.md#cronus-transport-publicrabbitmq-apiaddress)           | string                | no       |               |
| [Cronus:Transport:PublicRabbitMQ:UseAsyncDispatcher](configuration.md#cronus-transport-publicrabbitmq-useasyncdispatcher) | bool            | no       | false         |
| [Cronus:Transport:PublicRabbitMQ:FederatedExchange](configuration.md#cronus-transport-publicrabbitmq-federatedexchange) | configuration section | no   | `{ MaxHops = 1 }` |

The semantics of each key match the corresponding one on the private connection; the only differences are:

* `UseAsyncDispatcher` defaults to `false`.
* `FederatedExchange` is always initialised (to `{ MaxHops = 1 }`) rather than left null, because public events are typically federated across brokers.
* `PublicRabbitMqOptions.GetUpstreamUris()` parses `Server` with `AmqpTcpEndpoint.ParseMultiple` and combines it with `VHost` to build the upstream URIs for federation.

{% code title="appsettings.json" %}
```json
{
  "cronus": {
    "transport": {
      "publicrabbitmq": {
        "server": "rabbitmq.public.example.com",
        "port": 5672,
        "vhost": "/public",
        "username": "public-service",
        "password": "secret",
        "apiaddress": "https://rabbitmq.public.example.com:15672",
        "federatedexchange": {
          "maxhops": 1
        }
      }
    }
  }
}
```
{% endcode %}

## Cronus.Transport.RabbitMQ — Consumer

Consumer-side settings, bound from [`RabbitMqConsumerOptions`](https://github.com/Elders/Cronus.Transport.RabbitMQ/blob/master/src/Elders.Cronus.Transport.RabbitMQ/RabbitMqConsumerOptions.cs) under the `cronus:transport:rabbitmq:consumer` section. This replaces the legacy flat `Cronus:Transport:RabbitMq:ConsumerWorkersCount` setting that earlier versions used.

| Name                                                                                                      | Type | Required | Default Value |
| --------------------------------------------------------------------------------------------------------- | ---- | -------- | ------------- |
| [Cronus:Transport:RabbitMQ:Consumer:WorkersCount](configuration.md#cronus-transport-rabbitmq-consumer-workerscount) | int  | no       | 10            |
| [Cronus:Transport:RabbitMQ:Consumer:RpcTimeout](configuration.md#cronus-transport-rabbitmq-consumer-rpctimeout)     | int  | no       | 10            |
| [Cronus:Transport:RabbitMQ:Consumer:FanoutMode](configuration.md#cronus-transport-rabbitmq-consumer-fanoutmode)     | bool | no       | false         |

#### Cronus:Transport:RabbitMQ:Consumer:WorkersCount

The number of worker threads each Cronus consumer (application services, projections, ports, sagas, gateways, triggers) spins up for processing messages pulled from RabbitMQ. Validated with `[Range(1, int.MaxValue)]`.

#### Cronus:Transport:RabbitMQ:Consumer:RpcTimeout

The timeout, in seconds, Cronus waits for a response when dispatching an RPC call over RabbitMQ.

#### Cronus:Transport:RabbitMQ:Consumer:FanoutMode

Drastically changes the infrastructure layout. When enabled, each node creates its own queue and every message is delivered to every node, i.e. the bus behaves as a fan-out rather than a worker pool. Use with care; the default `false` gives you classic competing-consumers semantics.

{% code title="appsettings.json" %}
```json
{
  "cronus": {
    "transport": {
      "rabbitmq": {
        "consumer": {
          "workerscount": 10,
          "rpctimeout": 10,
          "fanoutmode": false
        }
      }
    }
  }
}
```
{% endcode %}

## Cronus.AtomicAction.Redis

The Redis-backed implementation of `Cronus.AtomicAction` uses distributed locks following the [Redlock algorithm](https://redis.io/topics/distlock). The `cronus:atomicaction:redis` configuration section is bound by **two** option providers in this package:

* [`RedisAtomicActionOptions`](https://github.com/Elders/Cronus.AtomicAction.Redis/blob/master/src/Elders.Cronus.AtomicAction.Redis/Config/RedisAtomicActionOptions.cs) — the atomic-action TTLs and connection string.
* [`RedLockOptions`](https://github.com/Elders/RedLock/blob/master/src/RedLock/RedLockOptions.cs) (from the `Elders.RedLock` library) — the Redlock-specific retry and drift settings.

Both classes read from the same section, so every key below goes into the same `cronus:atomicaction:redis` object in `appsettings.json`.

### RedisAtomicActionOptions

| Name                                                                                | Type     | Required | Default Value   |
| ----------------------------------------------------------------------------------- | -------- | -------- | --------------- |
| [Cronus:AtomicAction:Redis:ConnectionString](configuration.md#cronus-atomicaction-redis-connectionstring) | string   | yes      |                 |
| [Cronus:AtomicAction:Redis:LockTtl](configuration.md#cronus-atomicaction-redis-lockttl)     | TimeSpan | no       | 00:00:01.000    |
| [Cronus:AtomicAction:Redis:LongTtl](configuration.md#cronus-atomicaction-redis-longttl)     | TimeSpan | no       | 00:00:05.000    |

#### Cronus:AtomicAction:Redis:ConnectionString

Connection string to the Redis instance (or cluster) that holds the locks. Marked `[Required]`.

#### Cronus:AtomicAction:Redis:LockTtl

The TTL applied at the start of an atomic action. While this lock is held no other node may execute an action against the same aggregate root + revision. Defaults to `1 second`.

#### Cronus:AtomicAction:Redis:LongTtl

A second, longer TTL applied after the atomic action has succeeded. It prevents a late-arriving node from overwriting the last action on the same aggregate + revision. Defaults to `5 seconds` and does not interfere with the normal action flow.

### RedLockOptions

| Name                                                                                                   | Type     | Required | Default Value   |
| ------------------------------------------------------------------------------------------------------ | -------- | -------- | --------------- |
| [Cronus:AtomicAction:Redis:ConnectionString](configuration.md#cronus-atomicaction-redis-connectionstring) | string | yes      |                 |
| [Cronus:AtomicAction:Redis:LockRetryCount](configuration.md#cronus-atomicaction-redis-lockretrycount)  | ushort   | no       | 1               |
| [Cronus:AtomicAction:Redis:LockRetryDelay](configuration.md#cronus-atomicaction-redis-lockretrydelay)  | TimeSpan | no       | 00:00:00.010    |
| [Cronus:AtomicAction:Redis:ClockDriveFactor](configuration.md#cronus-atomicaction-redis-clockdrivefactor) | double | no       | 0.01            |

#### Cronus:AtomicAction:Redis:LockRetryCount

How many times the Redlock client retries acquiring a contended lock before giving up. Defaults to `1`.

#### Cronus:AtomicAction:Redis:LockRetryDelay

How long the client waits between retries. Defaults to `10 ms`.

#### Cronus:AtomicAction:Redis:ClockDriveFactor

Safety margin used by Redlock to compensate for clock drift between Redis nodes. See the [Redlock safety arguments](https://redis.io/docs/manual/patterns/distributed-locks/#safety-arguments) for the full derivation. Defaults to `0.01`.

{% code title="appsettings.json" %}
```json
{
  "cronus": {
    "atomicaction": {
      "redis": {
        "connectionstring": "redis:6379",
        "lockttl": "00:00:01.000",
        "longttl": "00:00:05.000",
        "lockretrycount": 1,
        "lockretrydelay": "00:00:00.010",
        "clockdrivefactor": 0.01
      }
    }
  }
}
```
{% endcode %}
