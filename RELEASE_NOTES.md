#### 1.1.42-rc12 - 15.12.2014
* Bug: Properly increment message's age on error

#### 1.1.42-rc11 - 15.12.2014
* Make the TransportMessage error origin to depend on ID of type string

#### 1.1.42-rc10 - 15.12.2014
* IMessageProcessor is not generic interface anymore

#### 1.1.42-rc09 - 15.12.2014
* Tests for MessageProcessor

#### 1.1.42-rc08 - 06.12.2014
* Completely rework IEventStore and IMessageProcessor (MessageStream)

#### 1.1.42-rc07 - 02.12.2014
* Fixed bug when registering handlers Try2

#### 1.1.42-rc06 - 02.12.2014
* Fixed bug when registering handlers

#### 1.1.42-rc05 - 29.11.2014
* Add BoundedContext to the AggregateCommit

#### 1.1.42-rc04 - 28.11.2014
* Refactor the Aggregate repository

#### 1.1.42-rc03 - 04.11.2014
* Release container resources for singleton objects

#### 1.1.42-rc02 - 03.11.2014
* Fix bug with state version

#### 1.1.42-rc01 - 31.10.2014
* Revert the the endpoint check

#### 1.1.42-beta9- 30.10.2014
* Check the pipeline endpoint before trying to close it

#### 1.1.42-beta8- 30.10.2014
* Check the pipeline endpoint before trying to close it

#### 1.1.42-beta7- 09.10.2014
* Fix bug with registering message handlers

#### 1.1.42-beta6 - 09.10.2014
* Add overload methods to configure named publishers

#### 1.1.42-beta5 - 09.10.2014
* Add overload methods to configure named consumers

#### 1.1.42-beta4 - 09.10.2014
* Add overload methods to configure named consumers

#### 1.1.42-beta3 - 08.10.2014
* Fix IMessageHandlerProcessor registration in IOC

#### 1.1.42-beta2 - 08.10.2014
* Rework the initialization of the consumer instances

#### 1.1.42-beta1 - 08.10.2014
* Remove pipeline and endpoint strategies because they are rabbitmq specific

#### 1.1.42-alpha10 - 08.10.2014
* Remove pipeline and endpoint strategies because they are rabbitmq specific

#### 1.1.42-alpha9 - 07.10.2014
* Split Pipeline and Endpoint conventions

#### 1.1.42-alpha8 - 07.10.2014
* Copy the container to all setting classes

#### 1.1.42-alpha7 - 07.10.2014
* Refactor all settings and how we build objects.

#### 1.1.42-alpha6 - 07.10.2014
* You can specify consumer name using the extenstion methods

#### 1.1.42-alpha5 - 07.10.2014
* You can now specify a consumer name in ConsumerSettings

#### 1.1.42-alpha4 - 06.10.2014
* Add the ioc container to the Cronus settings

#### 1.1.42-alpha3 - 06.10.2014
* Add simple ioc container

#### 1.1.42-alpha2 - 05.10.2014
* Introduce IMessageHandlerFactory

#### 1.1.42-alpha1 - 05.10.2014
* Move the event store configuration to the command consumer

#### 1.1.41 - 02.10.2014
* Fix bug with CB

#### 1.1.40 - 01.10.2014
* Rework how we use the aggregate Ids

#### 1.1.39 - 29.09.2014
* Remove the EventStorePublisher

#### 1.1.38 - 10.09.2014
* Moved rabbitmq to its own repository