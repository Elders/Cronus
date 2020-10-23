# Configuration

## Overview

By default, Cronus and its sub-components have good default settings. However, not everything can be auto-configured, such as connection strings to databases or endpoints to various services.

## Cronus

| Name | Type | Required | Default Value |
| :--- | :--- | :--- | :--- |
| [Cronus:BoundedContext](configuration.md#cronus-boundedcontext) | string | yes |  |
| [Cronus:Tenants](configuration.md#cronus-tenants-greater-than-greater-than-string-or-required-yes) | string\[\] | yes |  |
| [Cronus:ApplicationServicesEnabled](configuration.md#cronus-applicationservicesenabled) | bool | no | true |
| [Cronus:ProjectionsEnabled](configuration.md#cronus-projectionsenabled) | bool | no | true |
| [Cronus:PortsEnabled](configuration.md#cronus-portsenabled) | bool | no | true |
| [Cronus:SagasEnabled](configuration.md#cronus-sagasenabled) | bool | no | true |
| [Cronus:GatewaysEnabled](configuration.md#cronus-gatewaysenabled) | bool | no | true |

#### Cronus:BoundedContext

Cronus uses this setting to personalize your application. This setting is used for naming the following components:

* RabbiMQ exchange and queue names
* Cassandra EventStore names
* Cassandra Projection store names

Allowed Characters: `Cronus:BoundedContext` must be an alphanumeric character or underscore only: `^\b([\w\d_]+$)`'

#### Cronus:Tenants

List of tenants allowed to use the system. Cronus is designed with multitenancy in mind from the beginning and requires at least one tenant to be configured in order to work properly. The multitenancy aspects are applied to many components and to give you a feel about this here is an incomplete list of different parts of the system using this setting:

* Message - every message which is sent through Cronus is bound to a specific _tenant_
* RabbitMQ exchanges and queues are tenant aware
* Event Store - every tenant has a separate storage
* Projection Store - every tenant has a separate storage

Each value you provide in the array is converted and used further to lower.

Allowed Characters: `Cronus:Tenants` must be alphanumeric character or underscore only: `^\b([\w\d_]+$)`'

Example value: `["tenant1","tenant2","tenant3"]`

Once set you could use [`TenantsOptions`](https://github.com/Elders/Cronus/tree/f14b4918aa5862a73a0789cc868b5f08258410ea/src/Elders.Cronus/Multitenancy/TenantsOptions.cs) object via Dependency Injection for other purposes.

#### Cronus:ApplicationServicesEnabled

Specifies whether to start a consumer for the Application Services

#### Cronus:ProjectionsEnabled

Specifies whether to start a consumer for the Projections

#### Cronus:PortsEnabled

Specifies whether to start a consumer for the Ports

#### Cronus:SagasEnabled

Specifies whether to start a consumer for the Sagas

#### Cronus:GatewaysEnabled

Specifies whether to start a consumer for the Gateways

## Cronus.Api

| Name | Type | Required | Default Value |
| :--- | :--- | :--- | :--- |
| [Cronus:Api:Kestrel](configuration.md#hosting) | configurationSection | no |  |
| [Cronus:Api:JwtAuthentication](configuration.md#authentication) | configurationSection | no |  |

### Hosting

The API is hosted with Kestrel.

By default, the API is hosted on port `7477`.

A configuration could be provided by [KestrelOptions](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-3.0#kestrel-options). You can supply them directly in the DI or through a configuration file.

#### Cronus:Api:Kestrel

```text
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

### Authentication

The API could be protected using a JWT bearer authentication.

The configuration is provided by [JwtBearerOptions](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.jwtbeareroptions?view=aspnetcore-1.1&viewFallbackFrom=aspnetcore-2.2). You can supply them directly in the DI or through a configuration file.

#### Cronus:Api:JwtAuthentication

```text
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

Remarks: [https://stackoverflow.com/a/58736850/224667](https://stackoverflow.com/a/58736850/224667)

## Cronus.Persistence.Cassandra

| Name | Type | Required | Default Value |
| :--- | :--- | :--- | :--- |
| [Cronus:Persistence:Cassandra:ConnectionString](configuration.md#cronus-persistence-cassandra-connectionstring) | string | yes |  |
| [Cronus:Persistence:Cassandra:ReplicationStrategy](configuration.md#cronus-persistence-cassandra-replicationstrategy) | string | no | simple |
| [Cronus:Persistence:Cassandra:ReplicationFactor](configuration.md#cronus-persistence-cassandra-replicationstrategy) | int | no | 1 |
| [Cronus:Persistence:Cassandra:Datacenters](configuration.md#cronus-persistence-cassandra-replicationstrategy) | string\[\] | no |  |

#### Cronus:Persistence:Cassandra:ConnectionString

The connection to the Cassandra database server

#### Cronus:Persistence:Cassandra:ReplicationStrategy

Configures Cassandra replication strategy. This setting has effect only in the first run when creating the database.

Valid values:

* simple
* network\_topology - when using this setting you need to specify `Cronus:Persistence:Cassandra:ReplicationFactor` and  `Cronus:Persistence:Cassandra:Datacenters` as well

## Cronus.Projections.Cassandra

#### `Cronus:Projections:Cassandra:ConnectionString` &gt;&gt; _string \| Required: Yes_

The connection to the Cassandra database server

#### `Cronus:Projections:Cassandra:ReplicationStrategy` &gt;&gt; _string \| Required: No \| Default: simple_

Configures Cassandra replication strategy. This setting has effect only in the first run when creating the database.

Valid values:

* simple
* network\_topology - when using this setting you need to specify `Cronus:Projections:Cassandra:ReplicationFactor` and  `Cronus:Projections:Cassandra:Datacenters` as well

#### `Cronus:Projections:Cassandra:ReplicationFactor` &gt;&gt; _integer \| Required: No \| Default: 1_

#### `Cronus:Projections:Cassandra:Datacenters` &gt;&gt; _string\[\] \| Required: No_

#### `Cronus:Projections:Cassandra:TableRetention:DeleteOldProjectionTables` &gt;&gt; _boolean \| Required: No \| Default: true_

#### `Cronus:Projections:Cassandra:TableRetention:NumberOfOldProjectionTablesToRetain` &gt;&gt; _unsigned integer \| Required: No \| Default: 2_

## Cronus.Transport.RabbitMq

#### `Cronus:Transport:RabbiMQ:ConsumerWorkersCount` &gt;&gt; _integer \| Required: Yes \| Default: 5_

Configures the number of threads which will be dedicated for consuming messages from RabbitMQ for _every_ consumer.

#### `Cronus:Transport:RabbiMQ:Server` &gt;&gt; _string \| Required: Yes \| Default: 127.0.0.1_

DNS or IP to the RabbitMQ server

#### `Cronus:Transport:RabbiMQ:Port` &gt;&gt; _integer \| Required: Yes \| Default: 5672_

The port number on which the RabbitMQ server is running

#### `Cronus:Transport:RabbiMQ:VHost` &gt;&gt; _string \| Required: Yes \| Default: /_

The name of the virtual host. It is a good practice to not use the default `/` vhost. For more details see the [official docs](https://www.rabbitmq.com/vhosts.html). Cronus is not using this for managing multitenancy.

#### `Cronus:Transport:RabbiMQ:Username` &gt;&gt; _string \| Required: Yes \| Default: guest_

The RabbitMQ username

#### `Cronus:Transport:RabbiMQ:Password` &gt;&gt; _string \| Required: Yes \| Default: guest_

The RabbitMQ password

#### `Cronus:Transport:RabbiMQ:AdminPort` &gt;&gt; _integer \| Required: Yes \| Default: 5672_

RabbitMQ admin port used to create, delete rabbitmq resources

## Cronus.AtomicAction.Redis

An implementation of `Cronus.AtomicAction` using distributed locks with Redis

\(_Source:_ [https://redis.io/topics/distlock](https://redis.io/topics/distlock)\)

#### `Cronus:AtomicAction:Redis:ConnectionString` &gt;&gt; _string \| Required: Yes_

Configures the connection string where Redis is located

#### `Cronus:AtomicAction:Redis:LockTtl` &gt;&gt; _TimeSpan \| Required: No \| Default: 00:00:01.000_

#### `Cronus:AtomicAction:Redis:ShorTtl` &gt;&gt; _TimeSpan \| Required: No \| Default: 00:00:01.000_

#### `Cronus:AtomicAction:Redis:LongTtl` &gt;&gt; _TimeSpan \| Required: No \| Default: 00:00:05.000_

#### `Cronus:AtomicAction:Redis:LockRetryCount` &gt;&gt; _int \| Required: No \| Default: 3_

#### `Cronus:AtomicAction:Redis:LockRetryDelay` &gt;&gt; _TimeSpan \| Required: No \| Default: 00:00:00.100_

#### `Cronus:AtomicAction:Redis:ClockDriveFactor` &gt;&gt; _double \| Required: No \| Default: 0.01_

