# Overview
By default Cronus and its sub-components have good default settings. However not everything could be auto configured, such as connection strings to databases or endpoints to various services.

# Cronus
#### `cronus_boundedcontext` >> *string | Required: Yes*
Cronus uses this setting to personalize your application. Once set you could use [`BoundedContext`](src/Elders.Cronus/BoundedContext.cs) object via Dependency Injection for other purposes. This setting is used to name the following components:
* RabbiMQ exchange and queue names
* Cassandra EventStore names
* Cassandra Projection store names

---

#### `cronus_tenants` >> *string[] | Required: yes*
List of tenants allowed to use the system. Cronus is designed with multitenancy fromthe beginning and requires at least one tenant to be configured in order to work properly. The multitenancy aspects are applied to every component and to give you a sense here is a incomplete list of the components which are using this setting
* Message - every message which is sent through Cronus is bound to a specific *tenant*
* RabbitMQ exchanges and queues are tenant aware
* Event Store - every tenant has a separate storage
* Projection Store - every tenant has a separate storage

Example: `"tenant1,tenant2,tenant3"`

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

---


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

