# Ecosystem



### Legend

| Name | Description |
| :--- | :--- |
| [![Elders](https://camo.githubusercontent.com/433c7116356a11dcdc7b843a1fa8a249cda41d24/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f5374617475732d6f6c796d7075732d677265656e2e737667)](https://camo.githubusercontent.com/433c7116356a11dcdc7b843a1fa8a249cda41d24/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f5374617475732d6f6c796d7075732d677265656e2e737667) | It is stable and it will continue to get support, maintenance and future development |
| [![Elders](https://camo.githubusercontent.com/73024a9829e3eb60f8d6aac16a1ffdb68951de63/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f5374617475732d737479782d6f72616e67652e737667)](https://camo.githubusercontent.com/73024a9829e3eb60f8d6aac16a1ffdb68951de63/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f5374617475732d737479782d6f72616e67652e737667) | The future is not clear. There are two possible paths from here - olympus or tartarus |
| [![Elders](https://camo.githubusercontent.com/075bcd5b5fff87b2242c63065c11e808c26bdc7e/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f5374617475732d74617274617275732d7265642e737667)](https://camo.githubusercontent.com/075bcd5b5fff87b2242c63065c11e808c26bdc7e/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f5374617475732d74617274617275732d7265642e737667) | abandoned |

### Domain Modeling / Core

| Name | Links | Status | Description |
| :--- | :--- | :--- | :--- |
| DomainModeling | [code](https://github.com/Elders/Cronus.DomainModeling) [![NuGet](https://camo.githubusercontent.com/963941c2943b1e45ec6c20ab4c8c76bd597804f8/68747470733a2f2f696d672e736869656c64732e696f2f6e756765742f762f43726f6e75732e446f6d61696e4d6f64656c696e672e737667)](https://www.nuget.org/packages/Cronus.DomainModeling) [![GitHub issues](https://camo.githubusercontent.com/173ce5706be493d06f1bf8b8af9a3bd214a70d04/68747470733a2f2f696d672e736869656c64732e696f2f6769746875622f6973737565732f456c646572732f43726f6e75732e446f6d61696e4d6f64656c696e672f736869656c64732e737667)](https://github.com/Elders/Cronus.DomainModeling/issues) [![GitHub pull requests](https://camo.githubusercontent.com/0e2e3bb75e003bd5f00fef382163297592c95573/68747470733a2f2f696d672e736869656c64732e696f2f6769746875622f6973737565732d70722f456c646572732f43726f6e75732e446f6d61696e4d6f64656c696e672e737667)](https://github.com/Elders/Cronus.DomainModeling/pulls) | [![Elders](https://camo.githubusercontent.com/433c7116356a11dcdc7b843a1fa8a249cda41d24/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f5374617475732d6f6c796d7075732d677265656e2e737667)](https://camo.githubusercontent.com/433c7116356a11dcdc7b843a1fa8a249cda41d24/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f5374617475732d6f6c796d7075732d677265656e2e737667) | Contracts for DDD/CQRS development |
| Cronus | [code](https://github.com/Elders/Cronus) [![NuGet](https://camo.githubusercontent.com/73d23fafd233f007be2588602a2116bcf3b6c067/68747470733a2f2f696d672e736869656c64732e696f2f6e756765742f762f43726f6e75732e737667)](https://www.nuget.org/packages/Cronus) | [![Elders](https://camo.githubusercontent.com/433c7116356a11dcdc7b843a1fa8a249cda41d24/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f5374617475732d6f6c796d7075732d677265656e2e737667)](https://camo.githubusercontent.com/433c7116356a11dcdc7b843a1fa8a249cda41d24/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f5374617475732d6f6c796d7075732d677265656e2e737667) | Cronus is a lightweight framework for dispatching and receiving messages between microservices with DDD/CQRS in mind |

### Messaging

| Name | Status | Description |
| :--- | :--- | :--- |
| [RabbitMQ](https://github.com/Elders/Cronus.Transport.RabbitMQ) | olympus | It works so well that we do not need to implement other messaging. |

### Event store persistence

| Store | Status | Description |
| :--- | :--- | :--- |
| [Cassandra](https://github.com/Elders/Cronus.Persistence.Cassandra) | olympus | This persister is in production since 2013 and it is recommended for production purposes. |
| [MSSQL](https://github.com/Elders/Cronus.Persistence.MSSQL) | styx | The persister has been used in production with Cronus v1 but MSSQL is relational database and it does not fit well as an event store persister. |
| [GIT](https://github.com/Elders/Cronus.Persistence.Git-) | tartarus | Persister exists just for fun. |

### Serialization

| Serializer | Status | Description |
| :--- | :--- | :--- |
| [Json](https://github.com/Elders/Cronus.Serialization.NewtonsoftJson) | olympus | It is recommended to use the serializer with DataContracts |
| [Protobuf \(Proteus\)](https://github.com/Elders/Cronus.Serialization.Proteus) | styx | This has been the prefered serialization with Cronus v2. However, there is a huge warm up performance hit with big projects which needs to be resolved. Despite this, it works really fast. The implementation has small protocol changes |

### Projections persistence

| Store | Status | Description |
| :--- | :--- | :--- |
| [Cassandra](https://github.com/Elders/Cronus.Projections.Cassandra) | olympus | Stores projections in Cassandra |
| [ElasticSearch](https://github.com/Elders/Cronus.Projection.ElasticSearch) | olympus | Builds projections dynamically. Very usefull for projects which just started and changes occur frequently. However, it must be switched to another persister such as Cassandra after the initial stages of the project |
| [AtomicAction](https://github.com/Elders/Cronus.AtomicAction.Redis) | olympus | Aggregate distributed lock with Redis |

### Other

| Name | Status | Description |
| :--- | :--- | :--- |
| [Hystrix](https://github.com/Elders/Cronus.Hystrix) | olympus | Circuit breaker middleware for Cronus |
| [Migrations](https://github.com/Elders/Cronus.Migration.Middleware) | olympus | Middleware to handle migrations of any kind |
| [AtomicAction](https://github.com/Elders/Cronus.AtomicAction.Redis) | olympus | Aggregate distributed lock with Redis |

