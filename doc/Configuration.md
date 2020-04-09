# Overview
By default Cronus and its sub-components have good default settings. However not everything could be auto configured, such as connection strings to databases or endpoints to various services.

# Cronus
#### `Cronus:BoundedContext` >> *string | Required: Yes*
Cronus uses this setting to personalize your application. This setting is used for naming the following components:
* RabbiMQ exchange and queue names
* Cassandra EventStore names
* Cassandra Projection store names

Allowed Characters: `Cronus:BoundedContext` must be alphanumeric character or underscore only: `^\b([\w\d_]+$)`'

---

#### `Cronus:Tenants` >> *string[] | Required: yes*
List of tenants allowed to use the system. Cronus is designed with multitenancy in mind from the beginning and requires at least one tenant to be configured in order to work properly. The multitenancy aspects are applied to many components and to give you a feel about this here is an incomplete list of different parts of the system using this setting:
* Message - every message which is sent through Cronus is bound to a specific *tenant*
* RabbitMQ exchanges and queues are tenant aware
* Event Store - every tenant has a separate storage
* Projection Store - every tenant has a separate storage

Each value you provide in the array is converted and used further to lower. 

Allowed Characters: `Cronus:Tenants` must be alphanumeric character or underscore only: `^\b([\w\d_]+$)`'

Example value: `["tenant1","tenant2","tenant3"]`

Once set you could use [`TenantsOptions`](../src/Elders.Cronus/Multitenancy/TenantsOptions.cs) object via Dependency Injection for other purposes.

---

#### `Cronus:ApplicationServicesEnabled` >> *boolean | Required: No | Default: True*
Specifies whether to start a consumer for the Application Services

---

#### `Cronus:ProjectionsEnabled` >> *boolean | Required: No | Default: True*
Specifies whether to start a consumer for the Projections

---

#### `Cronus:PortsEnabled` >> *boolean | Required: No | Default: True*
Specifies whether to start a consumer for the Ports

---

#### `Cronus:SagasEnabled` >> *boolean | Required: No | Default: True*
Specifies whether to start a consumer for the Sagas

---

#### `Cronus:GatewaysEnabled` >> *boolean | Required: No | Default: True*
Specifies whether to start a consumer for the Gateways

# Cronus.Api

## Hosting
The api is hosted with Kestrel.

By default the Api is hosted on port `7477`.

A configuration could be provided by [KestrelOptions](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-3.0#kestrel-options). You can supply them directly in the DI or through a configuration file.

#### `Cronus:Api:Kestrel` >> *configurationSection | Required: no*

```
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

## Authentication
The API could be protected using a JWT bearer authentication.

The configuration is provided by [JwtBearerOptions](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.jwtbeareroptions?view=aspnetcore-1.1&viewFallbackFrom=aspnetcore-2.2). You can supply them directly in the DI or through a configuration file.

#### `Cronus:Api:JwtAuthentication` >> *configurationSection | Required: no*

```
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

Remarks: https://stackoverflow.com/a/58736850/224667

---

# Cronus.Persistence.Cassandra

#### `Cronus:Persistence:Cassandra:ConnectionString` >> *string | Required: Yes*
The connection to the Cassandra database server

---

#### `Cronus:Persistence:Cassandra:ReplicationStrategy` >> *string | Required: No | Default: simple*
Configures Cassandra replication strategy. This setting has effect only in the first run when creating the database.

Valid values:
* simple
* network_topology - when using this setting you need to specify `Cronus:Persistence:Cassandra:ReplicationFactor` and  `Cronus:Persistence:Cassandra:Datacenters` as well

---

#### `Cronus:Persistence:Cassandra:ReplicationFactor` >> *integer | Required: No | Default: 1*

---

#### `Cronus:Persistence:Cassandra:Datacenters` >> *string[] | Required: No*

# Cronus.Projections.Cassandra

#### `Cronus:Projections:Cassandra:ConnectionString` >> *string | Required: Yes*
The connection to the Cassandra database server

---

#### `Cronus:Projections:Cassandra:ReplicationStrategy` >> *string | Required: No | Default: simple*
Configures Cassandra replication strategy. This setting has effect only in the first run when creating the database.

Valid values:
* simple
* network_topology - when using this setting you need to specify `Cronus:Projections:Cassandra:ReplicationFactor` and  `Cronus:Projections:Cassandra:Datacenters` as well

---

#### `Cronus:Projections:Cassandra:ReplicationFactor` >> *integer | Required: No | Default: 1*

---

#### `Cronus:Projections:Cassandra:Datacenters` >> *string[] | Required: No*


# Cronus.Transport.RabbitMq

#### `Cronus:Transport:RabbiMQ:ConsumerWorkersCount` >> *integer | Required: Yes | Default: 5*
Configures the number of threads which will be dedicated for consuming messages from RabbitMQ for *every* consumer.

---

#### `Cronus:Transport:RabbiMQ:Server` >> *string | Required: Yes | Default: 127.0.0.1*
DNS or IP to the RabbitMQ server

---

#### `Cronus:Transport:RabbiMQ:Port` >> *integer | Required: Yes | Default: 5672*
The port number on which the RabbitMQ server is running

---

#### `Cronus:Transport:RabbiMQ:VHost` >> *string | Required: Yes | Default: /*
The name of the virtual host. It is a good practice to not use the default `/` vhost. For more details see the [official docs](https://www.rabbitmq.com/vhosts.html). Cronus is not using this for managing multitenancy.

---

#### `Cronus:Transport:RabbiMQ:Username` >> *string | Required: Yes | Default: guest*
The RabbitMQ username

---

#### `Cronus:Transport:RabbiMQ:Password` >> *string | Required: Yes | Default: guest*
The RabbitMQ password

---

#### `Cronus:Transport:RabbiMQ:AdminPort` >> *integer | Required: Yes | Default: 5672*
RabbitMQ admin port used to create, delete rabbitmq resources


# Cronus.AtomicAction.Redis
An implementation of `Cronus.AtomicAction` using distributed locks with Redis

(*Source:* https://redis.io/topics/distlock)

#### `Cronus:AtomicAction:Redis:ConnectionString` >> *string | Required: Yes*
Configures the connection string where Redis is located

#### `Cronus:AtomicAction:Redis:LockTtl` >> *TimeSpan | Required: No | Default: 00:00:01.000*

#### `Cronus:AtomicAction:Redis:ShorTtl` >> *TimeSpan | Required: No | Default: 00:00:01.000*

#### `Cronus:AtomicAction:Redis:LongTtl` >> *TimeSpan | Required: No | Default: 00:00:05.000*

#### `Cronus:AtomicAction:Redis:LockRetryCount` >> *int | Required: No | Default: 3*

#### `Cronus:AtomicAction:Redis:LockRetryDelay` >> *TimeSpan | Required: No | Default: 00:00:00.100*

#### `Cronus:AtomicAction:Redis:ClockDriveFactor` >> *double | Required: No | Default: 0.01*

