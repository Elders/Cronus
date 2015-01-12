#### 2.0.0-alpha05 - 12.01.2015
* Fix bug when serializing Exception type

#### 2.0.0-alpha04 - 12.01.2015
* Properly register cronus and domain modeling in Proteus

#### 2.0.0-alpha03 - 12.01.2015
* Update to Proteus 1.0.4
* Fix Proteus registrations

#### 2.0.0-alpha02 - 06.01.2015
* Fix nuget dependencies

#### 2.0.0-alpha01 - 06.01.2015
* Replace Protoreg with Proteus

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