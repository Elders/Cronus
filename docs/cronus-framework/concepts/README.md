# Concepts

Cronus is a framework for building services that are honest about the way their business works. Three ideas are present in every Cronus service and are worth reading about before the rest of the documentation:

* [**Domain-Driven Design**](ddd.md) — is what the services are _about_. Cronus assumes you have already decided what your core domain is, worked out the ubiquitous language with domain experts, and have settled on the boundaries of each bounded context. Without that, the rest of the framework is a set of answers to questions you have not asked yet.
* [**Event Sourcing**](es.md) — is how Cronus remembers what happened. Aggregates emit events when they change; events are appended to an immutable log; any current-state representation (the aggregate itself, a projection, an audit report) is derived from that log by replaying events. The log is the source of truth and everything else is disposable.
* [**CQRS**](cqrs.md) — is how Cronus separates writes from reads. The write side is a stack of aggregates, application services and an event store tuned for appends. The read side is a stack of projections tuned for the queries each UI actually makes. Commands and queries live on different types, often on different hosts, scaled independently.

You can build a Cronus service without being deeply fluent in all three — but you cannot build a _good_ one. The rest of this documentation assumes the vocabulary of these three pages.
