#### 4.0.0-beta0005 - 10.01.2018
* Adds ISerializer to the transport

#### 4.0.0-beta0004 - 05.01.2018
* Adds net461 target framework

#### 4.0.0-beta0003 - 21.08.2017
* Code cleanup

#### 4.0.0-beta0002 - 19.08.2017
* InMemoryAggregateRootAtomicAction implementation

#### 4.0.0-beta0001 - 17.08.2017
* This release uses the official netstandard 2.0

#### 3.0.3 - 03.05.2017
* Throws a specific exception when an AR cannot be loaded a.k.a invalid ARID

#### 3.1.0 - 20.10.2017
* Adjustments for AzureBus integration
* Uses latest Nyx build scripts

#### 3.0.6 - 14.09.2017
* Add ctor in AggregateCommit to pass Timestamp

#### 3.0.5 - 14.09.2017
* Move ProjectionCommit in Cronus

#### 3.0.4 - 01.09.2017
* I think we should definitely rewrite the RabbitMQ integration

#### 3.0.3 - 03.05.2017
* Throws a specific exception when an AR cannot be loaded a.k.a invalid ARID

#### 3.0.2 - 08.11.2016
* Remove default configuration for In Memory Aggregate Atomic Action

#### 3.0.1 - 22.09.2016
* Updates DomainModeling

#### 3.0.0 - 08.09.2016
* The entire workflow was reworked with Middleware execution pipeline
* Middleware for inmemory retries
* Consumers and Endpoint Factory now uses the SubscriptionsMiddlewares
* Adds support for Sagas
* Adds MessageId, CausationId, CorrelationId to all CronusMessages
* Adds validation checks when initializing EndpointConsumer
* Subscribers now care for MessageTypes of System.Type
* Uses consumer when building endpoints for propper transport initialization
* Fixed the PublishTimestamp header of the CronusMessage
* Properly destroy the container
* MessageThreshold checks removed.
* Perses: Reworked subscribers and subscription middleware. We can now support dynamic subscribing, and we can now also decuple rabbitmq specific logic for building queues etc.
* Perses: Rename TransportMessage to CronusMessage. There is a breaking change because of reorganization of the the props.

 #### 2.6.3 - 12.07.2016
* Fixed bug where Container.IsRegistered does not checks the singleton and the scoped registrations.
* Replaces the ConcurrentDictionary as a mechanizm for synchronizing with MemoryCache. The motivation behind this change is that we never invalidate the values but with MemoryCache we use sliding 30 seconds policy. In addition the MemoryCache is configured with 500mb memory cap and 10% of total physical memory cap.


#### 2.6.2 - 06.04.2016
* Fixed bug where Container.IsRegistered does not checks the singleton and the scoped registrations.

#### 2.6.1 - 19.03.2016
* Set default EventStreamIntegrityPolicy when Cronus starts. Do this inside the ctor of CronusSettings.

#### 2.6.0 - 19.03.2016
* Set default EventStreamIntegrityPolicy when Cronus starts.
* Send the publish delay directly with the EndpointMessage.
* When message is published we now attach GUID byte array as Base64 string in the message headers. Also if a message is schedules
or published with delay the publish timestamp is also attached to the message headers.
* Introduce EventStreamIntegrityPolicy which should take care about validation upon AggregateRoot loading. The resolvers only
apply InMemory fixes without writing to the database. At the moment this policy is a fact only in the UnitTests because we
need a configuration settings for this feature.

#### 2.5.0 - 19.02.2016
* Add additional ctors for AggregateCommit and mark the current as obsolete

#### 2.4.0 - 22.01.2016
* Remove ICronusPlayer. The new interface IEventStorePlayer provides everything for replaying events. #94
* Fix bug where PipelineConsumerWork throw unnecessary exceptions when endpoint is closed. #87
* Remove log4net dependency by using LibLog #95
* Moved DefaultAggregateRootAtomicAction, IAggregateRootLock and IRevisionStore to the Cronus.AtocmicAction.Redis project.
* Added support for cluster configuration and atomic action. In memory atomic action by default.
* Prepare for new implementation of Aggregate Atomic Action #93
* Disposable MessageHander #96 #97

#### 2.3.0 - 28.09.2015
* SetMessageProcessorName extension method

#### 2.2.5 - 06.07.2015
* Publish the real event when EntityEvent comes

#### 2.2.4 - 06.07.2015
* Update DomainModeling

#### 2.2.3 - 03.07.2015
* Update DomainModeling

#### 2.2.2 - 02.07.2015
* Update DomainModeling

#### 2.2.1 - 02.07.2015
* Update DomainModeling

#### 2.1.1 - 23.06.2015
* Fix issue with AR revision lock

#### 2.1.0 - 18.06.2015
* Add method TryLoad when loading aggregates from the event store

#### 2.0.0 - 15.05.2015
* Externalize the serialization into a separate nuget package

#### 1.2.17 - 04.16.2015
* Replaying events now returns unordered list of AggregateCommit

#### 1.2.16 - 04.04.2015
* Replaying events now returns the entire AggregateCommit

#### 1.2.15 - 27.03.2015
* Add Subscription name and remove MessageHanderType from public members of the subscription.

#### 1.2.14 - 25.03.2015
* Fix minor issue with hosting a application services

#### 1.2.13 - 25.03.2015
* Fix broken contract with RabbitMQ

#### 1.2.12 - 25.03.2015
* MessageProcessor now works with Message
* Introduce different MessageProcessorSubsctipyions for each type of handlers.

#### 1.2.11 - 23.03.2015
* Minor fixes and added checks

#### 1.2.10 - 13.03.2015
* MessageProcessor now exposes its subscriptions instead of handler types

#### 1.2.9 - 13.03.2015
* Fix issue with AggregateAtomicAction

#### 1.2.8 - 13.03.2015
* Initialize AggregateRepository only for CommandConsumer

#### 1.2.7 - 13.03.2015
* Cronus is now responsible for initializing the AggregateRepository

#### 1.2.6 - 12.03.2015
* Remove the AggregateRevisionService and put really simple lock

#### 1.2.5 - 12.03.2015
* Failed to release properly the package

#### 1.2.4 - 15.01.2015
* Fixed issue when unsubscribing from MessageProcessor

#### 1.2.3 - 15.01.2015
* Fixed minor issues.

#### 1.2.2 - 16.12.2014
* Fixed bug with registering types in the serializer

#### 1.2.1 - 16.12.2014
* Fixed bug with registering handlers

#### 1.2.0 - 16.12.2014
* Add ability to specify a consumer name in ConsumerSettings.
* Add simple ioc container (not a real ioc container).
* Add BoundedContext to the AggregateCommit.
* Rework the Aggregate repository.
* Rework the IEventStore interface.
* Rework the IMessageProcessor interface with observables.
* Rework all settings to use the IContainer.
* Fix bug with aggregate root state version.
* Remove pipeline and endpoint strategies because they are rabbitmq specific.
* Split Pipeline and Endpoint conventions.
* Introduce IMessageHandlerFactory.
* Move the event store configuration inside the command consumer configuration.

#### 1.1.41 - 02.10.2014
* Fix bug with CB

#### 1.1.40 - 01.10.2014
* Rework how we use the aggregate Ids

#### 1.1.39 - 29.09.2014
* Remove the EventStorePublisher

#### 1.1.38 - 10.09.2014
* Moved rabbitmq to its own repository
