# Overview
By default Cronus and its sub-components have good default settings. However not everything could be auto configured, such as connection strings to databases or endpoints to various services.

# Configuration

## Cronus

## Cronus.Transport.RabbitMq

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

