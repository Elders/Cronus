# Overview
By default Cronus and its sub-components have good default settings. However not everything could be auto configured, such as connection strings to databases or endpoints to various services.

# Cronus
#### `cronus:boundedcontext` >> *string | Required: Yes*
Cronus uses this setting to personalize your application. Once set you could use [`BoundedContext`](../src/Elders.Cronus/BoundedContext.cs) object via Dependency Injection for other purposes. This setting is used to name the following components:
* RabbiMQ exchange and queue names
* Cassandra EventStore names
* Cassandra Projection store names

Allowed Characters: `cronus_boundedcontext` must be alphanumeric character or underscore only: `^\b([\w\d_]+$)`'


---

#### `cronus:tenants` >> *string[] | Required: yes*
List of tenants allowed to use the system. Cronus is designed with multitenancy in mind from the beginning and requires at least one tenant to be configured in order to work properly. The multitenancy aspects are applied to many components and to give you a feel about this here is an incomplete list of different parts of the system using this setting:
* Message - every message which is sent through Cronus is bound to a specific *tenant*
* RabbitMQ exchanges and queues are tenant aware
* Event Store - every tenant has a separate storage
* Projection Store - every tenant has a separate storage

Each value you provide in the array is converted and used further to lower. 

Allowed Characters: `cronus_tenants` must be alphanumeric character or underscore only: `^\b([\w\d_]+$)`'

Example value: `["tenant1","tenant2","tenant3"]`

Once set you could use [`TenantsOptions`](../src/Elders.Cronus/Multitenancy/TenantsOptions.cs) object via Dependency Injection for other purposes.

---

#### `cronus_applicationservices_enabled` >> *boolean | Required: No | Default: True*
Specifies whether to start a consumer for the Application Services

---

#### `cronus_projections_enabled` >> *boolean | Required: No | Default: True*
Specifies whether to start a consumer for the Projections

---

#### `cronus_ports_enabled` >> *boolean | Required: No | Default: True*
Specifies whether to start a consumer for the Ports

---

#### `cronus_sagas_enabled` >> *boolean | Required: No | Default: True*
Specifies whether to start a consumer for the Sagas

---

#### `cronus_gateways_enabled` >> *boolean | Required: No | Default: True*
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

#### `cronus_persistence_cassandra_connectionstring` >> *string | Required: Yes*
The connection to the Cassandra database server

---

#### `cronus_persistence_cassandra_replication_strategy` >> *string | Required: No | Default: simple*
Configures Cassandra replication strategy. This setting has effect only in the first run when creating the database.

Valid values:
* simple
* network_topology - when using this setting you need to specify `cronus_persistence_cassandra_replication_factor` and  `cronus_persistence_cassandra__datacenters` as well

---

#### `cronus_persistence_cassandra_replication_factor` >> *integer | Required: No | Default: 1*

---

#### `cronus_persistence_cassandra__datacenters` >> *string[] | Required: No*

# Cronus.Projections.Cassandra

#### `cronus_projections_cassandra_connectionstring` >> *string | Required: Yes*
The connection to the Cassandra database server

---

#### `cronus_projections_cassandra_replication_strategy` >> *string | Required: No | Default: simple*
Configures Cassandra replication strategy. This setting has effect only in the first run when creating the database.

Valid values:
* simple
* network_topology - when using this setting you need to specify `cronus_projections_cassandra_replication_factor` and  `cronus_projections_cassandra__datacenters` as well

---

#### `cronus_projections_cassandra_replication_factor` >> *integer | Required: No | Default: 1*

---

#### `cronus_projections_cassandra__datacenters` >> *string[] | Required: No*


# Cronus.Transport.RabbitMq

#### `cronus_transport_rabbimq_consumer_workerscount` >> *integer | Required: Yes | Default: 5*
Configures the number of threads which will be dedicated for consuming messages from RabbitMQ for *every* consumer.

---

#### `cronus_transport_rabbimq_server` >> *string | Required: Yes | Default: 127.0.0.1*
DNS or IP to the RabbitMQ server

---

#### `cronus_transport_rabbimq_port` >> *integer | Required: Yes | Default: 15672*
The port number on which the RabbitMQ server is running

---

#### `cronus_transport_rabbimq_vhost` >> *string | Required: Yes | Default: /*
The name of the virtual host. It is a good practice to not use the default `/` vhost. For more details see the [official docs](https://www.rabbitmq.com/vhosts.html). Cronus is not using this for managing multitenancy.

---

#### `cronus_transport_rabbimq_username` >> *string | Required: Yes | Default: guest*
The RabbitMQ username

---

#### `cronus_transport_rabbimq_password` >> *string | Required: Yes | Default: guest*
The RabbitMQ password

---

#### `cronus_transport_rabbimq_adminport` >> *integer | Required: Yes | Default: 5672*
RabbitMQ admin port used to create, delete rabbitmq resources


# Cronus.AtomicAction.Redis

#### `cronus_atomicaction_redis_connectionstring` >> *string | Required: Yes *
Configures the connection string where Redis is located

#### `cronus_atomicaction_redis_ttl_lock_ms` >> *double | Required: No | Default: 1000 ms *

#### `cronus_atomicaction_redis_ttl_short_ms` >> *double | Required: No | Default: 1000 ms *

#### `cronus_atomicaction_redis_ttl_long_ms` >> *double | Required: No | Default: 300000 ms *
