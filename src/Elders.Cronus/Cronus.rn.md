#### 6.2.0 - 30.09.2020
* Move the projection rebuild operation to the cluster
* Changes the visibility if the ClusterJob DataOverride to private
* Fixes ProjectionVersion comparison

#### 6.1.1 - 17.09.2020
* Adds debug logging when rebuilding a projection

* Replaces the standard projection ProjectionBuilder with a cluster job
* Basic in-memory implementations

#### 6.1.0 - 24.08.2020
* Fixes registration for aspnet core applications
* Start experimenting with Activity
* Improves logging
* Improves publish/handle logs with structured logging
* Adds support for trigger handlers
* Adds a retry strategy when message publishing fails
* Adds support for PublicEvents generate by an Aggregate. The events are stored in AggregateCommit [#203]
* Imporoves logging extension methods

#### 6.0.1 - 23.04.2020
* Fixes event store manager state when the rebuild finishes successfully
* Automatically start the IEventStoreIndex handlers
* Updates Cronus.DomainModeling to v6.0.1

#### 6.0.0 - 16.04.2020
* Replaces LibLog [#188]
* Using CronusLogger for static logging
* Registers an EmptyConsumer by default for the IConsumer<>. Other components such as RabbitMQ will have the responsibility to override this. [#216]
* Extending the IEventStorePlayer interface with async methods
* Introduces options pattern for all configurations.
* Register a type container with discovered commands
* Adds bounded context header when publishing a message
* Respects if a messageId has been explicitly specified when publishing a message
* Bumps to dotnet core 3.1
* Fixes event indices by properly plugging in a workflow
* Added InMemory implementation for CronusJobRunner
* Added discovery for all CronusJobs and RebuildIndex_EventToAggregateRootId_JobFactory( explicitly depended in EventStoreIndexBuilder)
* Changed 'DiscoveryScanner' to not inherit 'DiscoveryBase<>'
* Renamed 'DiscoverFromAssemblies' to 'Scan' which returns a collection of IDiscoveryResult<>
* Changed CronusServiceCollectionExtensions to affect the changes above
* Adds an option directly to add services to IServiceCollection
* Initializes the commits collection
* Bypasses the event store index check when ISystemProjection is rebuilt
* Allows manual overriding of a Cronus Job data
* Handles the EventStoreIndexIsNowPresent event in the AR
* Adds tenant resolver which gets the tenant from a string source
* Adds EventStoreDiscoveryBase which registers event store indices
* Reworks the MultitenancyDiscovery
* Refactors tenant resolver dependencies
* Introduces Cronus jobs which are intended to run in a cluster
* Improves the EventStore interfaces so when you do page the store the reult contains a pagination token
* Rewrites the event store index with ARs and projections

#### 5.3.1 - 21.03.2019
* Fixes a concurrency issue when working with versions => https://github.com/dotnet/corefx/pull/28225
* Added ordering in projections registrations
* Simplified InMemoryProjectionVersionStore
* Added using in the InMemoryPublisher
* InMemoryPublisher that publishes commands & events in the correct order with Correlation Id
* The Projection Builder now has the right order for publishing Timeouts.
* Projection Versions are now added properly.

#### 5.3.0 - 28.01.2019
* Uses async version for loading a Snapshot
* Improves error logging
* ProjectionVersions does not inherit ICollection anymore. Adds more responsibilities to that class as well

#### 5.2.3 - 23.01.2019
* Fixes the initilization of ProjectionRepository collection

#### 5.2.2 - 23.01.2019
* Fixes which projection versions will be used for persistence

#### 5.2.1 - 22.01.2019
* Fixes a projection bug

#### 5.2.0 - 10.01.2019
* Adds CronusOptionsProviderBase which allows easy options setup
* Drops net472 because netstandard2.0 supports it out of the box
* Adds async load methods for projections ISnapshotStore
* Fixed loading assemblies on linux since its case sensitive.
* Fixed a SingleOrDefault blowup when an Assembly is loaded twice - for some reason, when using xUnit, xunit is loaded twice. 
* Adds InMemory implementations for ILock and Publisher
* Adds startup ranking via CronusStartupAttribute
* Introduces InMemory publisher
* Registers all ICronusStartup services
* Adds extension point to write cronus bootstrap logic
* Introduces DiscoveryBase as a successor of DiscoveryBasedOnExecutingDirAssemblies
* Introduces InMemoryDiscovery which adds default in memory services
* Adds the ability non-default Cronus services to override the defaults
* Projections will replay for a long time until we resolve performance issues
* Changed Subscriber Collection Implementation and the way cronus host work
* Added Synchronous Message Processor

#### 5.1.0 - 10.12.2018
* Updates DomainModeling
* Updates to DNC 2.2
* Fixes a bug where you were not able to rebuild a projection without live state
* Improves the logs for the ProjectionRepositoryWithFallback
* Creates an extension point where the client application could override Cronus services

#### 5.0.1 - 04.12.2018
* Added Multitenancy configurations format validation and support for providing tenants as Json Array

#### 5.0.0 - 29.11.2018
* Improves logging for the ProjectionRepositoryWithFallback
* Fixes execution flow for the projection which had mixed understandings about not found data and an error when doing a projection query
* Outdated build versions are now canceled
* Adds logging when ProjectionRepositoryWithFallback is fired
* Handles the situation where a projection does not exist and sets the version status to NotPresent
* Adds the ability to override how ProjectionVersions are loaded
* Removes the Initialize option because it is in a separate class now
* Fixes concurrency problem with the Workflow objects
* Fixes handler initialization bug. Handlers must be initialized using the handlerFactory
* Splits IProjectionWriter and IInitializeProjectionStore
* Introduces ProjectionRepositoryWithFallback. It gives the ability to use a secondary/fallback projection repository. It is useful while rebuilding the projections
* Now we can replay system projections
* Refreshes the projections status every 5 min
* Adds more context to the AggregateCommitRaw
* Introduces CopyEventStore class
* Adds an option to read the event store without deserializing the data to an object
* Adds migration discovery
* Move the Cronus.Migration.Middleware repository inside Cronus
* Adds generic interfaces `IEventStore<TSettings>` and `IEventStorePlayer<TSettings>`
* Added tenant resolve on Cronus Message handlerTypeContainer
* Added CronusHostOptions from which you can disable Application Services, Projections, Ports, Sagas or Gateways explicitly 
* Replaces handle logging with DiagnosticsWorkflow
* ProjectionRepository is now creating the handler instances using the IHandlerFactory
* Adds BoundedContext which represents the configuration setting cronus_boundedcontext so that other services can get it injected directly
* ProjectionVersions are now per tenant. Based on the client's tenant configuration there will be commands issued upon start. If a client removes one tenant frpm the configuration there is no need to rebuild/replay the projections for that tenant.
* Adds IndexStatus parser
* Fixes index rebuild flow
* Extends the IEventStoreStorageManager to support indices creating
* Rework how the current tenant is set during message handling
* Every message is now consumed inside a DI scope
* Removes the abstract modifier from the CronusHost
* Introduces CronusServiceCollectionExtensions
* Fixes how assemblies are loaded from the executing dir
* Removes all consumers and moved them to he RabbitMQ project because they were too specific about how RabbitMQ works. With this the MultiThreading.Scheduler is removed
* Adds MS dependency injection
* Moves the ISerializer to Elders.Cronus namespace
* Changes the IDiscovery interface to have a specific discovery target like IDiscovery<ISerializer>
* Reworks the discovery interface
* Adds Async Functionality to IProjectionLoader
* Fixes the assembly name
* Makes ProjectionStream internal
* Added ILock and InMemoryLock implementation
* Do not clear processed aggregates on every event type
* Get base 64 string once
* Log total commits after projection rebuilding finishes
* Do not clear processed aggregates on every event type
* Get base 64 string once
* Cancel building projection version
* IsOutdatad refactoring
* It is milliseconds, apparently.
* Delete redundant code
* Check the persisted index state before rebuilding
* Make sure ProjectionVersionsHandler will never be rebuilt
* Increase the rebuild timebox to 24 hours
* Mark ProjectionVersionsHandler as a system projection with ISystemProjection. Apparently we need it
* Register EventTypeIndexForProjections only if it hasn't been
* Do not mark ProjectionVersionsHandler as a system projection. Strange things are happening
* Improved logs for projection versioning
* Write events only for the specified version when rebuilding
* Mark ProjectionVersionsHandler as a system projection with ISystemProjection
* When rebuilding a projection version and it times out the result which is returned has additional context to indicate that this is a timeout
* Improved logging for RebuildProjection command
* Improved logging for rebuilding projections
* Always use global registration of InMemoryProjectionVersionStore
* Projection versions are not requested for rebuild if there are other versions already scheduled
* Persist index building status
* Logs an error message when an event could not be persisted in projection store for specific projection version. Other projection versions are not interrupted.
* Projection versions are not requested for rebuild if there are other versions already scheduled
* The snapshotStore is not queried anymore if the projection is not snapshotable
* When rebuilding a projection version and it times out the result which is returned has additional context to indicate that this is a timeout
* Outdated version builds are being canceled
* BREAKING: Replaces `PersistentProjectionVersionHandler` with `ProjectionVersionsHandler`
* Force rebuild projection
* Do not return snapshots for projections with `IAmNotSnapshotable`
* Adds `ProjectionVersionsHandler` which tracks all versions of a projection including full history
* Splits DefaultSnapshotStrategy into two. The EventsCountSnapshotStrategy is based only on number of projection events. The TimeOffsetSnapshotStrategy adds on top of EventsCountSnapshotStrategy the ability to create a snapshot based on time difference between the last written event and at the time of loading a projection
* Improves projection write performance. Projection state is now loaded only when a new state is going to be created
* Return the result from publishing a command
* Fixes an index creation problem. In addition we now ensure that only one index is built at the same moment (single node only)
* Projection rebuild is not terminated when the deadline hits but a want log messages is written
* Adds caching for processed aggregates. This potentially could result in out of memory. This is a temporary solution
* Logs for index rebuilding
* Properly stops consumer
* Adds support for Revision in ProjectionVersion
* Immediately aknowledge/consume message when it is delivered
* Properly create Empty instance of ProjectionStream
* IProjectionLoader and IProjectionRepository are registered in Cronus. Moved from Elders.Cronus.Projections.Cassandra
* Removes all obsolete code
* Removes Obsolete EventStore methods

#### 4.1.4 - 28.03.2018
* Fixes a bug related to projection store initialization

#### 4.1.3 - 28.03.2018
* Release build by Developer machine. For some reason the version 4.1.1 built by AppVeyor is not working properly.

#### 4.1.1 - 22.03.2018
* Log a Warning instead of an Error when loading assemblies dynamically for discovery

#### 4.1.0 - 22.03.2018
* Adds support for event store multitenancy
* Auto discovery feature which will automatically configure cronus settings

#### 4.0.11 - 13.03.2018
* Fixes an exception while working with ProjectionVersions collection

#### 4.0.10 - 26.02.2018
* Fixes projections where PersistentProjectionsHandler has a bad state

#### 4.0.9 - 26.02.2018
* Fixes project file and dependencies

#### 4.0.8 - 26.02.2018
* Updates DomainModeling package

#### 4.0.7 - 22.02.2018
* Fixes the projections middleware

#### 4.0.6 - 21.02.2018
* Improves logs for projections even more

#### 4.0.5 - 20.02.2018
* Improves logs for projections

#### 4.0.4 - 20.02.2018
* Adds real multitarget framework support for netstandard2.0;net45;net451;net452;net46;net461;net462

#### 4.0.3 - 19.02.2018
* Fixes handler subscription to not register duplicate handlers
* https://gyazo.com/51a9f27125ea8c5c8429929abbe2fe44

#### 4.0.2 - 19.02.2018
* Fixes handler subscription to not register duplicate handlers

#### 4.0.1 - 16.02.2018
* Configures the version request timeout to one hour
* The version hash for the projection index is now constant. Probably this will change in future when we want to rebuild the index in a separate storage
* Adds tracing for building projections
* Improves the ProjectionVersions with couple of guards and ToString() override

#### 4.0.0 - 12.02.2018
* Projections
* Properly set the NumberOfWorkers
* Adds ISerializer to the transport
* Adds net461 target framework
* InMemoryAggregateRootAtomicAction implementation
* Uses the official netstandard 2.0

#### 3.1.1 - 09.01.2018
* Adds endpoint per bounded context namespace convention
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
