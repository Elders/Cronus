# Table of contents

* [Introduction](README.md)

## Cronus Framework

* [Concepts](cronus-framework/concepts/README.md)
  * [Domain Driven Design](cronus-framework/concepts/ddd.md)
  * [Event Sourcing](cronus-framework/concepts/es.md)
  * [Command Query Responsibility Segregation](cronus-framework/concepts/cqrs.md)
* [Domain Modeling](cronus-framework/domain-modeling/README.md)
  * [Bounded Context](cronus-framework/domain-modeling/bounded-context.md)
  * [Multitenancy](cronus-framework/domain-modeling/multitenancy.md)
  * [Aggregate](cronus-framework/domain-modeling/aggregate.md)
  * [Entity](cronus-framework/domain-modeling/entity.md)
  * [Value Object](cronus-framework/domain-modeling/value-object.md)
  * [IDs](cronus-framework/domain-modeling/ids.md)
  * [Published Language](cronus-framework/domain-modeling/published-language.md)
  * [Messages](cronus-framework/domain-modeling/messages/README.md)
    * [Commands](cronus-framework/domain-modeling/messages/commands.md)
    * [Events](cronus-framework/domain-modeling/messages/events.md)
    * [Public Events](cronus-framework/domain-modeling/messages/public-events.md)
    * [Signals](cronus-framework/domain-modeling/messages/signals.md)
  * [Handlers](cronus-framework/domain-modeling/handlers/README.md)
    * [Application Services](cronus-framework/domain-modeling/handlers/application-services.md)
    * [Sagas](cronus-framework/domain-modeling/handlers/sagas.md)
    * [Projections](cronus-framework/domain-modeling/handlers/projections.md)
    * [Ports](cronus-framework/domain-modeling/handlers/ports.md)
    * [Triggers](cronus-framework/domain-modeling/handlers/triggers.md)
    * [Gateways](cronus-framework/domain-modeling/handlers/gateways.md)
* [Event Store](cronus-framework/event-store/README.md)
  * [EventStore Player](cronus-framework/event-store/eventstore-player.md)
  * [Migrations](cronus-framework/event-store/migrations/README.md)
    * [Copy EventStore](cronus-framework/event-store/migrations/copy-eventstore.md)
* [Workflows](cronus-framework/workflows.md)
* [Indices](cronus-framework/indices.md)
* [Jobs](cronus-framework/jobs.md)
* [Cluster](cronus-framework/cluster.md)
* [Messaging](cronus-framework/messaging/README.md)
  * [Serialization](cronus-framework/messaging/serialization.md)
* [Configuration](cronus-framework/configuration.md)
* [Unit testing](cronus-framework/unit-testing.md)
