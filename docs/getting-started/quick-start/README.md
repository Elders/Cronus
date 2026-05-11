---
description: >-
  Build a small task-management service end-to-end with Cronus: API, worker,
  commands, events, and projections.
---

# Quick Start

## Business requirements

To drive the examples in the following pages, we'll build a task-management service that meets these real-world requirements:

* A new task-management system for multiple tenants.
* Data must be consistent — no partial updates, no silent loss of state.
* Tasks can be reassigned inside the same user group.
* Every user has an accurate progress report.
* Group progress reports are restricted to group members.
* When a user finishes a task, the group is notified.
* A screen shows the historical changes in user activity.
* When a user closes their account, an optional exit survey is recorded.
* A monthly report summarises why lost users closed their accounts.

We won't implement every single bullet in the quick start. The goal is to get a working skeleton in place and see a command become an event become a projection.

## Path through the quick start

1. [Setup](setup.md) — create two processes (API + worker), add the Cronus packages, start Cassandra & RabbitMQ in Docker, and wire `AddCronus(Configuration)`.
2. [Persist first event](persist-first-event.md) — model `TaskAggregate`, publish a `CreateTask` command, persist a `TaskCreated` event.
3. [Explore projections](explore-projections.md) — build a `TaskProjection` over the events and query it from the API.

By the end you will have a two-process Cronus service you can grow into a real domain. Each page ends with a pointer to the deeper documentation on the building block it introduced.
