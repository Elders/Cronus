# [9.0.0-preview.48](https://github.com/Elders/Cronus/compare/v9.0.0-preview.47...v9.0.0-preview.48) (2023-06-21)


### Bug Fixes

* Exposes the Commits collection in AggregateStream ([f42d01a](https://github.com/Elders/Cronus/commit/f42d01a8383d58ba6f74461e943b4eb504b986e5))

# [9.0.0-preview.47](https://github.com/Elders/Cronus/compare/v9.0.0-preview.46...v9.0.0-preview.47) (2023-06-19)


### Bug Fixes

* Replaces ByteArrayLookup with array.IndexOf(Span) ([41f955a](https://github.com/Elders/Cronus/commit/41f955a51c0c6dd1c0db2cac43414cbcde2595ba))

# [9.0.0-preview.46](https://github.com/Elders/Cronus/compare/v9.0.0-preview.45...v9.0.0-preview.46) (2023-06-15)


### Bug Fixes

* Log MaxDegreeOfParallelism ([5624651](https://github.com/Elders/Cronus/commit/56246517ac992e0b5a234fdcfa02b0014e237ed1))

# [9.0.0-preview.45](https://github.com/Elders/Cronus/compare/v9.0.0-preview.44...v9.0.0-preview.45) (2023-06-15)


### Bug Fixes

* Multiply MaxDegreeOfParallelism by 100 ([40a08c0](https://github.com/Elders/Cronus/commit/40a08c08b08f6545248909decef8b2e2fdda5978))

# [9.0.0-preview.44](https://github.com/Elders/Cronus/compare/v9.0.0-preview.43...v9.0.0-preview.44) (2023-06-07)


### Bug Fixes

* Fixes unit tests for ProjectionVersions where the validation does not throw exception anymore ([22a7862](https://github.com/Elders/Cronus/commit/22a7862aabfe71d107d85b4446e091797b192d3c))


### Features

* Splits workflow execution to Create/Run/Complete ([be02438](https://github.com/Elders/Cronus/commit/be02438d1e07ed4c3106db452af786e427efd42e))

# [9.0.0-preview.43](https://github.com/Elders/Cronus/compare/v9.0.0-preview.42...v9.0.0-preview.43) (2023-06-06)


### Features

* Adds options for start/end date when replaying projections ([3c7225a](https://github.com/Elders/Cronus/commit/3c7225a53e9c0802b0b2c96a85922139e7a91cb7))

# [9.0.0-preview.42](https://github.com/Elders/Cronus/compare/v9.0.0-preview.41...v9.0.0-preview.42) (2023-05-05)


### Bug Fixes

* Disables event store index diagnostics ([1e6187b](https://github.com/Elders/Cronus/commit/1e6187b6955a8dd04c61e18f9c524bd5382b8f8d))

# [9.0.0-preview.41](https://github.com/Elders/Cronus/compare/v9.0.0-preview.40...v9.0.0-preview.41) (2023-04-22)


### Bug Fixes

* Fixes logger type ([43726b2](https://github.com/Elders/Cronus/commit/43726b2028497d93205cf65fc0dcc58b572d0764))
* Removes left over code which is slowing deserialization ([3fd3ab9](https://github.com/Elders/Cronus/commit/3fd3ab9ac4713e5cbc53a9a2c665446398288b0c))


### Features

* Reworks IEventStoreInterceptor to IAggregateInterceptor ([e5d9a97](https://github.com/Elders/Cronus/commit/e5d9a97c624914ee9f1e7f92b336bee9c4fcb437))

# [9.0.0-preview.40](https://github.com/Elders/Cronus/compare/v9.0.0-preview.39...v9.0.0-preview.40) (2023-04-21)


### Bug Fixes

* Fixes publicEventPublisher job when using raw messages ([54fbeb0](https://github.com/Elders/Cronus/commit/54fbeb03dfd86ad4f378ef9d81fb73c907002cf8))

# [9.0.0-preview.39](https://github.com/Elders/Cronus/compare/v9.0.0-preview.38...v9.0.0-preview.39) (2023-04-07)


### Reverts

* Revert "fix: Limits the rettry to 1" ([d68b7a9](https://github.com/Elders/Cronus/commit/d68b7a97791f43ce98c55e859ba92e4e623d696a))

# [9.0.0-preview.38](https://github.com/Elders/Cronus/compare/v9.0.0-preview.37...v9.0.0-preview.38) (2023-04-07)


### Bug Fixes

* Limits the rettry to 1 ([5ac941d](https://github.com/Elders/Cronus/commit/5ac941d76f09e802b703234d82eb408728eebef0))

# [9.0.0-preview.37](https://github.com/Elders/Cronus/compare/v9.0.0-preview.36...v9.0.0-preview.37) (2023-04-07)


### Features

* Optimizes the public event replays ([73051c5](https://github.com/Elders/Cronus/commit/73051c5499bb89a9886e22ffebff8d2eed4cc01f))

# [9.0.0-preview.36](https://github.com/Elders/Cronus/compare/v9.0.0-preview.35...v9.0.0-preview.36) (2023-04-05)


### Bug Fixes

* Fixes projections/indices startup ([79b08a4](https://github.com/Elders/Cronus/commit/79b08a401cc2b5bae8945feb8e2f536ce46e55e7))

# [9.0.0-preview.35](https://github.com/Elders/Cronus/compare/v9.0.0-preview.34...v9.0.0-preview.35) (2023-04-04)


### Bug Fixes

* call OnReplayCompletedAsync after projection is built ([ec0408a](https://github.com/Elders/Cronus/commit/ec0408a2bc746485215ad67faa3ef47d0ec6efca))

# [9.0.0-preview.34](https://github.com/Elders/Cronus/compare/v9.0.0-preview.33...v9.0.0-preview.34) (2023-03-31)


### Bug Fixes

* Fixes replay of the events ([c172e16](https://github.com/Elders/Cronus/commit/c172e16ee383ea5e2a1a4d0c404ad5f106e92086))

# [9.0.0-preview.33](https://github.com/Elders/Cronus/compare/v9.0.0-preview.32...v9.0.0-preview.33) (2023-03-29)


### Bug Fixes

* Adds ToJson() method for IndexRecord ([5a04d7d](https://github.com/Elders/Cronus/commit/5a04d7d24caf9d207fde4ed6ea3d06ff69d8e101))

# [9.0.0-preview.32](https://github.com/Elders/Cronus/compare/v9.0.0-preview.31...v9.0.0-preview.32) (2023-03-29)


### Bug Fixes

* Fixes null reference error when setting counter ([4f8f99a](https://github.com/Elders/Cronus/commit/4f8f99a9ad31ed46f3b7c03970f6466daa86e998))

# [9.0.0-preview.31](https://github.com/Elders/Cronus/compare/v9.0.0-preview.30...v9.0.0-preview.31) (2023-03-29)


### Bug Fixes

* Fixes ReplayPublicEvents_Job data sync with the cluster ([759a270](https://github.com/Elders/Cronus/commit/759a2701e45328ca32b5405cdfec9b8906c7dd1d))

# [9.0.0-preview.30](https://github.com/Elders/Cronus/compare/v9.0.0-preview.29...v9.0.0-preview.30) (2023-03-21)


### Features

* Cronus now properly is indexing the PublicEvents produced by the current bounded context ([33c90a2](https://github.com/Elders/Cronus/commit/33c90a2e35b6b6bf85a5688786fabab474b8157d))

# [9.0.0-preview.29](https://github.com/Elders/Cronus/compare/v9.0.0-preview.28...v9.0.0-preview.29) (2023-03-21)


### Bug Fixes

* update domainmodeling ([76167b1](https://github.com/Elders/Cronus/commit/76167b15922e07707c4223b13e9f7fa6ac15b8ca))

# [9.0.0-preview.28](https://github.com/Elders/Cronus/compare/v9.0.0-preview.27...v9.0.0-preview.28) (2023-03-07)


### Bug Fixes

* Consolidates publisher log and error handling ([24697ef](https://github.com/Elders/Cronus/commit/24697ef9b99d01e2c27ba2dc3c43c221cff2c695))

# [9.0.0-preview.27](https://github.com/Elders/Cronus/compare/v9.0.0-preview.26...v9.0.0-preview.27) (2023-02-08)


### Features

* Allows deleting events from the event store ([2bc303a](https://github.com/Elders/Cronus/commit/2bc303a9ff698378ded5fe017ec7919c9916bdf6))

# [9.0.0-preview.26](https://github.com/Elders/Cronus/compare/v9.0.0-preview.25...v9.0.0-preview.26) (2023-02-03)


### Bug Fixes

* Fixes how the projection store is initialized ([74bf3e5](https://github.com/Elders/Cronus/commit/74bf3e5893bf6be82574a2ffd30805ded5b17bc8))

# [9.0.0-preview.25](https://github.com/Elders/Cronus/compare/v9.0.0-preview.24...v9.0.0-preview.25) (2023-01-30)


### Bug Fixes

* Fixes a side affect  issue when building headers for a message. ([ba9a08a](https://github.com/Elders/Cronus/commit/ba9a08ab1273ac72a942eb2660a3f7f8dd5f566b))
* Fixes the public events replay job ([7d6d35e](https://github.com/Elders/Cronus/commit/7d6d35e99c9a86cf44b9fe6b1b06f258b4ba89bd))

# [9.0.0-preview.24](https://github.com/Elders/Cronus/compare/v9.0.0-preview.23...v9.0.0-preview.24) (2023-01-26)


### Bug Fixes

* Updates packages ([c211cb4](https://github.com/Elders/Cronus/commit/c211cb4caf074df25eacd54cc711819f99e04fd2))

# [9.0.0-preview.23](https://github.com/Elders/Cronus/compare/v9.0.0-preview.22...v9.0.0-preview.23) (2023-01-19)

# [9.0.0-preview.22](https://github.com/Elders/Cronus/compare/v9.0.0-preview.21...v9.0.0-preview.22) (2023-01-16)

# [9.0.0-preview.21](https://github.com/Elders/Cronus/compare/v9.0.0-preview.20...v9.0.0-preview.21) (2023-01-05)


### Bug Fixes

* Improves logging when there is a client error ([5b3ba2f](https://github.com/Elders/Cronus/commit/5b3ba2face3c9fb50cdca1b6280ad9a31ba51654))

# [9.0.0-preview.20](https://github.com/Elders/Cronus/compare/v9.0.0-preview.19...v9.0.0-preview.20) (2022-12-30)


### Bug Fixes

* Cleans the Error struct and adds some more options to construct it ([30d7652](https://github.com/Elders/Cronus/commit/30d7652fbac1f4fa93e38ca5ec8f5bf2273b3cfd))

# [9.0.0-preview.19](https://github.com/Elders/Cronus/compare/v9.0.0-preview.18...v9.0.0-preview.19) (2022-12-15)


### Bug Fixes

* Fixes hasMoreRecords check when building EventToAggregateRootId ([4b68e2f](https://github.com/Elders/Cronus/commit/4b68e2fc302e85d923802f7875f929a4e587403b))

# [9.0.0-preview.18](https://github.com/Elders/Cronus/compare/v9.0.0-preview.17...v9.0.0-preview.18) (2022-12-14)


### Features

* adds OnReplayCompleted method in GGPort ([4ce9ae4](https://github.com/Elders/Cronus/commit/4ce9ae40cf10cb32f6c62f790510100909f168f4))

# [9.0.0-preview.17](https://github.com/Elders/Cronus/compare/v9.0.0-preview.16...v9.0.0-preview.17) (2022-11-24)


### Bug Fixes

* Cronus interface for replaying public events ([0e6d9d1](https://github.com/Elders/Cronus/commit/0e6d9d1906b98809e95375096c6ae3a5e26e24bd))

# [9.0.0-preview.16](https://github.com/Elders/Cronus/compare/v9.0.0-preview.15...v9.0.0-preview.16) (2022-10-20)


### Bug Fixes

* Adds dirty fix about event position while indexing. This needs refactoring! ([1591e79](https://github.com/Elders/Cronus/commit/1591e794e8e084fc40c59578be3690ecc6c74ece))

# [9.0.0-preview.15](https://github.com/Elders/Cronus/compare/v9.0.0-preview.14...v9.0.0-preview.15) (2022-10-19)


### Bug Fixes

* Adds method to load PublicEvents when we do replay ([7dd7f3b](https://github.com/Elders/Cronus/commit/7dd7f3b7545b438c0592caaf4fb7f897357a1bcf))

# [9.0.0-preview.14](https://github.com/Elders/Cronus/compare/v9.0.0-preview.13...v9.0.0-preview.14) (2022-10-06)


### Bug Fixes

* Rebuild projection try with IAsyncEnumerable ([3d91577](https://github.com/Elders/Cronus/commit/3d91577fe9694bdce3c01f1d6298d74d55efd51c))

# [9.0.0-preview.13](https://github.com/Elders/Cronus/compare/v9.0.0-preview.12...v9.0.0-preview.13) (2022-10-06)


### Bug Fixes

* Try improving the replay performance ([2234cd9](https://github.com/Elders/Cronus/commit/2234cd90a8918632af11de451277a08f35b96422))

# [9.0.0-preview.12](https://github.com/Elders/Cronus/compare/v9.0.0-preview.11...v9.0.0-preview.12) (2022-10-06)


### Bug Fixes

* Removes batching when rebuilding projection ([b3ab726](https://github.com/Elders/Cronus/commit/b3ab7260b8a12e7c4c8293f643977032341d98e6))

# [9.0.0-preview.11](https://github.com/Elders/Cronus/compare/v9.0.0-preview.10...v9.0.0-preview.11) (2022-10-06)


### Bug Fixes

* Properly ping the cluster while counting messages ([164e2c8](https://github.com/Elders/Cronus/commit/164e2c8ba97efb256bb67d72d3bd81e80579c23f))

# [9.0.0-preview.10](https://github.com/Elders/Cronus/compare/v9.0.0-preview.9...v9.0.0-preview.10) (2022-10-06)


### Bug Fixes

* Ping every 5 seconds when doing the message counter ([8ccf7fa](https://github.com/Elders/Cronus/commit/8ccf7faf478b9e6e6b427f26adf40771b5e727e8))

# [9.0.0-preview.9](https://github.com/Elders/Cronus/compare/v9.0.0-preview.8...v9.0.0-preview.9) (2022-10-06)


### Bug Fixes

* Adds message counter ping for the cluster ([71afb49](https://github.com/Elders/Cronus/commit/71afb49eabc43a9f236ad8cf8ad207ab6e2d83e6))

# [9.0.0-preview.8](https://github.com/Elders/Cronus/compare/v9.0.0-preview.7...v9.0.0-preview.8) (2022-10-05)


### Bug Fixes

* Optimizes the message counter job. Some code cleanups ([2fe5548](https://github.com/Elders/Cronus/commit/2fe5548552b50070119df9a08d741221b983da13))

# [9.0.0-preview.7](https://github.com/Elders/Cronus/compare/v9.0.0-preview.6...v9.0.0-preview.7) (2022-10-04)


### Bug Fixes

* Code cleanup ([54844c6](https://github.com/Elders/Cronus/commit/54844c6e742f6cd4ca022c991aa8733cbe2ea1dc))
* Improve the async operations when indexing ([3a4cfe7](https://github.com/Elders/Cronus/commit/3a4cfe77562a98df94b92240a501dbd5dd201584))
* Removes SourceLink package because it is in the root dir ([5bb43c9](https://github.com/Elders/Cronus/commit/5bb43c91ebdd6af620eef77211bf0e9b4217522e))

# [9.0.0-preview.6](https://github.com/Elders/Cronus/compare/v9.0.0-preview.5...v9.0.0-preview.6) (2022-09-21)


### Bug Fixes

* Improve migrator execution process ([932cd90](https://github.com/Elders/Cronus/commit/932cd900a22bd2bd3da8705f7f492bf1d81dece1))

# [9.0.0-preview.5](https://github.com/Elders/Cronus/compare/v9.0.0-preview.4...v9.0.0-preview.5) (2022-09-15)


### Bug Fixes

* Code refactoring ([a8ca052](https://github.com/Elders/Cronus/commit/a8ca052dfa6d95fbe5cb139a5a097d9afe135589))

# [9.0.0-preview.4](https://github.com/Elders/Cronus/compare/v9.0.0-preview.3...v9.0.0-preview.4) (2022-09-15)


### Bug Fixes

* Minor rebuilding progress improvements ([463c151](https://github.com/Elders/Cronus/commit/463c151bd0cf8723c8befcd621c6a32b712bbbc9))

# [9.0.0-preview.3](https://github.com/Elders/Cronus/compare/v9.0.0-preview.2...v9.0.0-preview.3) (2022-09-14)


### Bug Fixes

* Try to deploy [#3](https://github.com/Elders/Cronus/issues/3) ([b9ff22d](https://github.com/Elders/Cronus/commit/b9ff22dbf8cbb7a834449fa64ac536a3e5d3062f))

# [9.0.0-preview.2](https://github.com/Elders/Cronus/compare/v9.0.0-preview.1...v9.0.0-preview.2) (2022-09-14)


### Bug Fixes

* Try to deploy [#2](https://github.com/Elders/Cronus/issues/2) ([357ffd7](https://github.com/Elders/Cronus/commit/357ffd7cb2141742d8994ba8c5d838ff9308f06d))

# [9.0.0-preview.1](https://github.com/Elders/Cronus/compare/v8.7.5...v9.0.0-preview.1) (2022-09-14)


### Bug Fixes

* Fix aggregate revision in tests ([60691a7](https://github.com/Elders/Cronus/commit/60691a7ac2029a5d6c73095192cab454e0ca26cc))


### Features

* Mark Index record as sealed and try to deploy ([0d4cb38](https://github.com/Elders/Cronus/commit/0d4cb38b5e5890cd35efe692527e09c9d86f32d8))

## [8.7.5](https://github.com/Elders/Cronus/compare/v8.7.4...v8.7.5) (2022-09-02)


### Bug Fixes

* Continues to index aggregate commit for a projection version when there is an error. ([58bc69b](https://github.com/Elders/Cronus/commit/58bc69b57d02e6069e934465a443d401196e0116))

## [8.7.4](https://github.com/Elders/Cronus/compare/v8.7.3...v8.7.4) (2022-08-31)


### Bug Fixes

* do not create instances of the projection when rebuilding it ([2bbdf7a](https://github.com/Elders/Cronus/commit/2bbdf7ad107613eb7b03f49438ade13384a92376))

## [8.7.3](https://github.com/Elders/Cronus/compare/v8.7.2...v8.7.3) (2022-08-30)


### Bug Fixes

* add proper exception when there is no aggregate root id in state ([41333d4](https://github.com/Elders/Cronus/commit/41333d4519770a50827fd1e5e0a30fcdd5f158a9))

## [8.7.2](https://github.com/Elders/Cronus/compare/v8.7.1...v8.7.2) (2022-08-29)


### Bug Fixes

* retry workflow works with async/awat ([ae58926](https://github.com/Elders/Cronus/commit/ae58926a94493055e2b4c4b7898cd860266873e7))

## [8.7.1](https://github.com/Elders/Cronus/compare/v8.7.0...v8.7.1) (2022-08-16)


### Bug Fixes

* pipeline update ([f054d67](https://github.com/Elders/Cronus/commit/f054d674d7dd2d2c5bfe050984f86224d1d59d96))

# [8.7.0](https://github.com/Elders/Cronus/compare/v8.6.1...v8.7.0) (2022-08-10)


### Features

* Fix previous feature build ([612ac4e](https://github.com/Elders/Cronus/commit/612ac4e7a21ac62a15f7323f74bd26a8434150fb))
* Tries to stay away from optimizations that could lead to the connections overflow ([c88b07a](https://github.com/Elders/Cronus/commit/c88b07a80f5bc109678142bcfa6b09c0b9ea25bb))

## [8.6.1](https://github.com/Elders/Cronus/compare/v8.6.0...v8.6.1) (2022-07-28)


### Bug Fixes

* Fix projection job progress ([acc90e0](https://github.com/Elders/Cronus/commit/acc90e0c4e6f5c9b1501aee609832cf44deb72d7))

# [8.6.0](https://github.com/Elders/Cronus/compare/v8.5.0...v8.6.0) (2022-07-22)


### Features

* Mark rebuild job progress signals as ISystemSignal ([ea0ab90](https://github.com/Elders/Cronus/commit/ea0ab90ac872eaf75efc99ae960cd013790549ac))

# [8.5.0](https://github.com/Elders/Cronus/compare/v8.4.1...v8.5.0) (2022-07-19)


### Features

* Auto recover missing projection store ([feb4fca](https://github.com/Elders/Cronus/commit/feb4fca9083288e3a3b393702b25a64f4fac9589))

## [8.4.1](https://github.com/Elders/Cronus/compare/v8.4.0...v8.4.1) (2022-07-18)


### Bug Fixes

* prevents NullReferenceException when a tenant resolver returns null ([d0839df](https://github.com/Elders/Cronus/commit/d0839dfe30879eda8f8521762ee794942376a611))

# [8.4.0](https://github.com/Elders/Cronus/compare/v8.3.1...v8.4.0) (2022-07-12)


### Features

* Auto-recover projections when using new projection store ([37725d2](https://github.com/Elders/Cronus/commit/37725d25e7a384eee3b9e6db694004148c87b441))

## [8.3.1](https://github.com/Elders/Cronus/compare/v8.3.0...v8.3.1) (2022-07-08)


### Bug Fixes

* Reworks TenantResolver ([fbb61a3](https://github.com/Elders/Cronus/commit/fbb61a34704f52aeb0ab079bd91768c609ba085b))

# [8.3.0](https://github.com/Elders/Cronus/compare/v8.2.0...v8.3.0) (2022-07-07)


### Bug Fixes

* Fix Rebuild increments projection version ([719e02e](https://github.com/Elders/Cronus/commit/719e02e138137db19b83897a043870490e39aaf2))
* semantic release, attempt 1 ([75cbd2c](https://github.com/Elders/Cronus/commit/75cbd2c21c94ab2893fd8c5bfaa40813669161fa))
* semantic release, attempt 2 ([b2d3469](https://github.com/Elders/Cronus/commit/b2d3469cc482bb3960d1f6c5e84e9df1c8907df6))
* semantic release, attempt 2 ([c2f1fe8](https://github.com/Elders/Cronus/commit/c2f1fe8288af7d9a2a5a95a4824162d9b45e868e))


### Features

* Update Domain Modeling ([70e4dd2](https://github.com/Elders/Cronus/commit/70e4dd2d8456eae1f1b93a0568476e0aeda10ce2))

# [8.2.0](https://github.com/Elders/Cronus/compare/v8.1.3...v8.2.0) (2022-06-24)


### Features

* Add Rpc Api to Cronus ([b02a2b9](https://github.com/Elders/Cronus/commit/b02a2b9175600c2cd30fd837593e1888865af052))

## [8.1.3](https://github.com/Elders/Cronus/compare/v8.1.2...v8.1.3) (2022-06-24)


### Bug Fixes

* Do not bootstrap projections and indices when they are explicitly disabled from configuration ([ce0069b](https://github.com/Elders/Cronus/commit/ce0069bb813aacd49ace68664160ae57dd152fda))

## [8.1.2](https://github.com/Elders/Cronus/compare/v8.1.1...v8.1.2) (2022-06-16)


### Bug Fixes

* enables rebuilding of projections that don't have a live version ([943701d](https://github.com/Elders/Cronus/commit/943701db8d4976a61c1c4f7f26cf7f32724f1d38))

## [8.1.1](https://github.com/Elders/Cronus/compare/v8.1.0...v8.1.1) (2022-06-15)


### Bug Fixes

* Fix overflow of processed count when rebuilding projcetion. ([a9bd003](https://github.com/Elders/Cronus/commit/a9bd003c840332395464647f462dcdaea41b1402))

# [8.1.0](https://github.com/Elders/Cronus/compare/v8.0.6...v8.1.0) (2022-06-15)
# [8.1.0-preview.1](https://github.com/Elders/Cronus/compare/v8.0.4...v8.1.0-preview.1) (2022-05-27)


### Features

* Ensure canceling of projections is working (for better experience we should deploy UI ) ([1186428](https://github.com/Elders/Cronus/commit/1186428b052f032a4bf33b2e5c5ec037a494a882))

## [8.0.6](https://github.com/Elders/Cronus/compare/v8.0.5...v8.0.6) (2022-05-31)


### Bug Fixes

* The information of the omitted aggregate commit is not important unless when debugging the framework ([988dbf0](https://github.com/Elders/Cronus/commit/988dbf0bdb07a6ac67beaeb54ebc2ef7c9b57711))

## [8.0.5](https://github.com/Elders/Cronus/compare/v8.0.4...v8.0.5) (2022-05-31)


### Bug Fixes

* Resolves a problem where a developer wants to remove an aggregate commit. It is actually a feature, maybe.... ([086ef83](https://github.com/Elders/Cronus/commit/086ef83332bd2fa1472cc1d1b90fe6a32c465aeb))
* Add Rpc host ([8b1e03b](https://github.com/Elders/Cronus/commit/8b1e03bc3b9e6b3a372adfbec9d599ec551f0363))
* Trigger pipeline ([3f76e8b](https://github.com/Elders/Cronus/commit/3f76e8b6cc816d81f070e832449702e984e31f83))

## [8.0.4](https://github.com/Elders/Cronus/compare/v8.0.3...v8.0.4) (2022-05-26)


### Bug Fixes

* Remove one of two indices startup ([ac8c277](https://github.com/Elders/Cronus/commit/ac8c2776c8a091fbf78240036bdab33fd3e5b0c7))

## [8.0.3](https://github.com/Elders/Cronus/compare/v8.0.2...v8.0.3) (2022-05-26)


### Bug Fixes

* Stop awaiting dynamic handles as we know that await waits for the execution to be completed ([022023e](https://github.com/Elders/Cronus/commit/022023ec7da1ecad5a85f157dd6433b916dc2a27))

## [8.0.2](https://github.com/Elders/Cronus/compare/v8.0.1...v8.0.2) (2022-05-26)


### Bug Fixes

* Async improvements ([4b490ff](https://github.com/Elders/Cronus/commit/4b490ff2791c9e9b82902339bbc1ee23d2698bd4))

## [8.0.1](https://github.com/Elders/Cronus/compare/v8.0.0...v8.0.1) (2022-05-25)


### Bug Fixes

* Update Domain Modeling ([6057f22](https://github.com/Elders/Cronus/commit/6057f2256780df4c0f5ca77c8133edd0b2fd7699))

# [8.0.0](https://github.com/Elders/Cronus/compare/v7.0.0...v8.0.0) (2022-05-25)


### Bug Fixes

* Add missing await ([96749d2](https://github.com/Elders/Cronus/commit/96749d20564fbe508e97ea10721ee25ad840fb8c))
* Another try ([915e022](https://github.com/Elders/Cronus/commit/915e02271fe8ac845936e78b442e8f14c59a7d7b))
* Awaits async invocations when building the message counter ([49c0aed](https://github.com/Elders/Cronus/commit/49c0aed626206d89edac79da51829c8cb81700c3))
* bump Microsoft.NET.Test.Sdk from 17.1.0 to 17.2.0 ([7aa80b3](https://github.com/Elders/Cronus/commit/7aa80b3b431d26f5a1adaa93644d0ab0fc034ebd))
* changelog fix and push version .13 ([eeddd4b](https://github.com/Elders/Cronus/commit/eeddd4bc2e9cac279cc1bb4e7619bc38c15f6aa7))
* Could you deploy yourself please ([8d59354](https://github.com/Elders/Cronus/commit/8d5935477de5203c96948002f9ff8cfaf77ebce6))
* Deploy ([91e6c60](https://github.com/Elders/Cronus/commit/91e6c6008843534aa65a5b5f94002b502c166556))
* Enable public events replay ([b67b4f8](https://github.com/Elders/Cronus/commit/b67b4f8bcc80be6d86ad3944a4b0c457670572a5))
* Fix not registered ILock ([dd82912](https://github.com/Elders/Cronus/commit/dd829123507905b8a6d818ed8b21da5fe75efe2f))
* Implement async SnapshotStore ([b075ae2](https://github.com/Elders/Cronus/commit/b075ae2639c73a19afb950973d29dde1c3331a2c))
* Improve Projection Store ([f1b3efb](https://github.com/Elders/Cronus/commit/f1b3efb5069adb8ddead2551e2ee5bb84b40c223))
* InitializeAsync ([d03d689](https://github.com/Elders/Cronus/commit/d03d689006b14ff66e59eeeacc4486ad2fb9c633))
* Load Aggregate Commits async ([0b66a21](https://github.com/Elders/Cronus/commit/0b66a216fe9369eb52d61703ef2386d538273f2f))
* Move some In-memory implementations to support only tests ([49cd3a5](https://github.com/Elders/Cronus/commit/49cd3a5388c6e3d8f2f720c2cafec86e52228547))
* Provide async message counter ([e23c79c](https://github.com/Elders/Cronus/commit/e23c79c985ff5d0a19f3f2f9c246b87d92fde18a))
* Return ILock back ([8384337](https://github.com/Elders/Cronus/commit/8384337b8fdc45bf1f419c19826a0b8f37780f8c))


### Reverts

* Revert "fix: Uses async to log an error message" ([575b3b8](https://github.com/Elders/Cronus/commit/575b3b8b8f58f5525968abd16ff407181c62ac27))

# [8.0.0-preview.14](https://github.com/Elders/Cronus/compare/v8.0.0-preview.13...v8.0.0-preview.14) (2022-04-26)

# [8.0.0-preview.13](https://github.com/Elders/Cronus/compare/v8.0.0-preview.12...v8.0.0-preview.13) (2022-04-21)


### Bug Fixes

* changelog fix and push version .13 ([eeddd4b](https://github.com/Elders/Cronus/commit/eeddd4bc2e9cac279cc1bb4e7619bc38c15f6aa7))

# [8.0.0-preview.12](https://github.com/Elders/Cronus/compare/v8.0.0-preview.11...v8.0.0-preview.12) (2022-04-21)


### Bug Fixes

* Awaits async invocations when building the message counter ([49c0aed](https://github.com/Elders/Cronus/commit/49c0aed626206d89edac79da51829c8cb81700c3))

### Reverts

* Revert "fix: Uses async to log an error message" ([575b3b8](https://github.com/Elders/Cronus/commit/575b3b8b8f58f5525968abd16ff407181c62ac27))

# [8.0.0-preview.11](https://github.com/Elders/Cronus/compare/v8.0.0-preview.10...v8.0.0-preview.11) (2022-04-18)


### Bug Fixes

* Add missing await ([96749d2](https://github.com/Elders/Cronus/commit/96749d20564fbe508e97ea10721ee25ad840fb8c))

# [8.0.0-preview.10](https://github.com/Elders/Cronus/compare/v8.0.0-preview.9...v8.0.0-preview.10) (2022-04-13)


### Bug Fixes

* Enable public events replay ([b67b4f8](https://github.com/Elders/Cronus/commit/b67b4f8bcc80be6d86ad3944a4b0c457670572a5))

# [8.0.0-preview.9](https://github.com/Elders/Cronus/compare/v8.0.0-preview.8...v8.0.0-preview.9) (2022-04-13)


### Bug Fixes

* Improve Projection Store ([f1b3efb](https://github.com/Elders/Cronus/commit/f1b3efb5069adb8ddead2551e2ee5bb84b40c223))

# [8.0.0-preview.8](https://github.com/Elders/Cronus/compare/v8.0.0-preview.7...v8.0.0-preview.8) (2022-04-12)


### Bug Fixes

* InitializeAsync ([d03d689](https://github.com/Elders/Cronus/commit/d03d689006b14ff66e59eeeacc4486ad2fb9c633))

# [8.0.0-preview.7](https://github.com/Elders/Cronus/compare/v8.0.0-preview.6...v8.0.0-preview.7) (2022-04-12)


### Bug Fixes

* Implement async SnapshotStore ([b075ae2](https://github.com/Elders/Cronus/commit/b075ae2639c73a19afb950973d29dde1c3331a2c))

# [8.0.0-preview.6](https://github.com/Elders/Cronus/compare/v8.0.0-preview.5...v8.0.0-preview.6) (2022-04-12)


### Bug Fixes

* Fix not registered ILock ([dd82912](https://github.com/Elders/Cronus/commit/dd829123507905b8a6d818ed8b21da5fe75efe2f))

# [8.0.0-preview.5](https://github.com/Elders/Cronus/compare/v8.0.0-preview.4...v8.0.0-preview.5) (2022-04-11)


### Bug Fixes

* Provide async message counter ([e23c79c](https://github.com/Elders/Cronus/commit/e23c79c985ff5d0a19f3f2f9c246b87d92fde18a))

# [8.0.0-preview.4](https://github.com/Elders/Cronus/compare/v8.0.0-preview.3...v8.0.0-preview.4) (2022-04-08)


### Bug Fixes

* Load Aggregate Commits async ([0b66a21](https://github.com/Elders/Cronus/commit/0b66a216fe9369eb52d61703ef2386d538273f2f))

# [8.0.0-preview.3](https://github.com/Elders/Cronus/compare/v8.0.0-preview.2...v8.0.0-preview.3) (2022-04-08)


### Bug Fixes

* Return ILock back ([8384337](https://github.com/Elders/Cronus/commit/8384337b8fdc45bf1f419c19826a0b8f37780f8c))

# [8.0.0-preview.2](https://github.com/Elders/Cronus/compare/v8.0.0-preview.1...v8.0.0-preview.2) (2022-04-08)


### Bug Fixes

* Move some In-memory implementations to support only tests ([49cd3a5](https://github.com/Elders/Cronus/commit/49cd3a5388c6e3d8f2f720c2cafec86e52228547))

# [8.0.0-preview.1](https://github.com/Elders/Cronus/compare/v7.0.0...v8.0.0-preview.1) (2022-04-08)

# [7.0.0](https://github.com/Elders/Cronus/compare/v6.4.3...v7.0.0) (2022-04-05)


### Bug Fixes

* Accelerate replay/rebuild job ([ae02d19](https://github.com/Elders/Cronus/commit/ae02d19139cc3662172eb66af6360607c438459a))
* Add Aggregate Commit transformation interface ([045c617](https://github.com/Elders/Cronus/commit/045c617ccc11e4239c156b06c6929a6fda75029c))
* Add heartbeat interval to settigns ([a09cd3f](https://github.com/Elders/Cronus/commit/a09cd3f13733fb3a0a69553774151df375f4db1a))
* Add interceptor for modifying upcomming aggregate commits ([55ba3f9](https://github.com/Elders/Cronus/commit/55ba3f9abd8b12639d09fee012d815234080f780))
* Add logging om handle error ([634e6ba](https://github.com/Elders/Cronus/commit/634e6ba2e7a0059d67a60e7775d4dd52f83e592d))
* Add Machine name to heartbeat ([49ae370](https://github.com/Elders/Cronus/commit/49ae3707acf8f438778b4db38baa3798d17b9602))
* Add timered logger for every Rebuild index operation [INT] ([fccd98c](https://github.com/Elders/Cronus/commit/fccd98c6628b08454d8864c22f60a8d0618e1e28))
* Add TTL to MessageHeaders ([00074c6](https://github.com/Elders/Cronus/commit/00074c61b4547c7691d132138b5e7f5c2adf0948))
* Adds retries and log when replaying projection ([f5ebdff](https://github.com/Elders/Cronus/commit/f5ebdff614924677b4cb7db2a927aa94a6310667))
* Adds try catches on all job runner methods ([41d42e5](https://github.com/Elders/Cronus/commit/41d42e5711926b96b26e0a1e68d72d602638ee24))
* And the other eye ([c98b74c](https://github.com/Elders/Cronus/commit/c98b74c4cbfb822079183dea7ede3c00ef746b64))
* Bootstrap Cronus before all services have started ([07d686d](https://github.com/Elders/Cronus/commit/07d686d8edef56ae52aefdc81ab7bf5f94c92691))
* Build fix ([175640e](https://github.com/Elders/Cronus/commit/175640ec6d8e7a0d505abf23a063724df214260e))
* bump version ([507111c](https://github.com/Elders/Cronus/commit/507111c73c5f7a388ceadca9ec11165f4fd81181))
* Change  NotFound projection message log level to Debug ([5c18665](https://github.com/Elders/Cronus/commit/5c186659c965682eb3c038634476c7443de97196))
* Change subscriber workflow for Triggers ([9e92912](https://github.com/Elders/Cronus/commit/9e929121edeb647cef151e247438bd6645b529ca))
* Configures the signal settings ([49cef65](https://github.com/Elders/Cronus/commit/49cef65603aed8840c115b84b0b748a139d43a31))
* Ensure service provider in ErrorContext ([24af784](https://github.com/Elders/Cronus/commit/24af78470a4af3baa515f6fd12412986b93a2661))
* Fix build ([a2f325c](https://github.com/Elders/Cronus/commit/a2f325c4e60212afb4714c6eed45257b59a8f08a))
* Fix error handling in async lamda + fix while loop condition ([e30b468](https://github.com/Elders/Cronus/commit/e30b46830c4c40fe9fb15de383cf0ecc207eec7a))
* Fixes Linux paths. _!_0x M$ ([e76ea49](https://github.com/Elders/Cronus/commit/e76ea490262243b5f15d7c9faba0f0c643548f5f))
* Fixes projection version building ([17e417e](https://github.com/Elders/Cronus/commit/17e417ecd5eda5426cf04799c43a7eec34c92f29))
* Fixes projection version handler not loading ([70b975e](https://github.com/Elders/Cronus/commit/70b975ec05ec0a577c909ecbdbce97cf6ca18080))
* Introducing live progress with fast signals ([cbf4704](https://github.com/Elders/Cronus/commit/cbf4704fc030a3ab2b98986e4dad5a7cb2318f26))
* Launch fast signals ([3b8d505](https://github.com/Elders/Cronus/commit/3b8d5052b2e6253081b6d3127813882d1a625d32))
* log elapsed time for indexing AR commits ([5c630c5](https://github.com/Elders/Cronus/commit/5c630c5b4846371ccec5c427eca23ce3d02f3f65))
* Log unsuccessful publish ([01fc3a9](https://github.com/Elders/Cronus/commit/01fc3a9eb7dbb4db1a22f4cb1250e2093fc1ea17))
* Makes the the AR id value format in the message header consistent everywhere ([daf2f31](https://github.com/Elders/Cronus/commit/daf2f312ed6d725c618afbc74d101272392c1a61))
* Moved to synchronous Start ([54e80f9](https://github.com/Elders/Cronus/commit/54e80f95698505d5d0ca71b6aacace857e5ea210))
* My eyes ([d9eb59d](https://github.com/Elders/Cronus/commit/d9eb59df07e706646170b793b9834a56e0875dfe))
* Removes obsolete log extensions ([5449e27](https://github.com/Elders/Cronus/commit/5449e27f88775efd74ddb45ff83cb2a9a3d3e166))
* Removes retries ([ab489d2](https://github.com/Elders/Cronus/commit/ab489d2816ef91bf094c058ea0a9ea02b9ce0f42))
* Revert "fix: Makes the the AR id value format in the message header consistent everywhere" ([2136fbc](https://github.com/Elders/Cronus/commit/2136fbcb9e25c93ec486c8b0aaf8d5ec14af4fb4))
* Reverts the diagnostic logs because they create a log of noise at the moment ([386c224](https://github.com/Elders/Cronus/commit/386c224be56984917e9aada2f1ac68778df66345))
* Rolls back how projections are rebuilt ([f02a065](https://github.com/Elders/Cronus/commit/f02a065c611d03e1368f59c36f03135f64c3a2d8))
* Sending heartbeat twice, since now every service should use AddCronusHeartbeat ([604058f](https://github.com/Elders/Cronus/commit/604058f7bba5c63148072122e9cd67e621ec8f1c))
* Transitions to Assembly.Location from Assembly.CodeBase ([b6b4a94](https://github.com/Elders/Cronus/commit/b6b4a948c7a10ee6ccb997b8bfa2ed184d93045d))
* update Domain Modeling package ([b994800](https://github.com/Elders/Cronus/commit/b9948006f2db568c894c03f35b17d931a12a3916))
* Update Domain Modeling package version ([17ab387](https://github.com/Elders/Cronus/commit/17ab387a6510dc803d95e309eca8da0c7dc2fb37))
* Update Domain Modeling package version ([3ac9b1c](https://github.com/Elders/Cronus/commit/3ac9b1c5f2f39533ec14afb16382e1daed8b8dea))


### Features

* add EnvironmentConfig field ([a2357c7](https://github.com/Elders/Cronus/commit/a2357c731357eb095c29492017d816b10d51c7f3))
* Add snapshot skipping optimization for not snapshotable projections ([5d2771b](https://github.com/Elders/Cronus/commit/5d2771b8fc45d7bc375bde3672dba67faaa05f7e))
* Adds heartbeat ([60f703e](https://github.com/Elders/Cronus/commit/60f703e4a75fb01cfea8b2193127219dc173b325))
* Adds the ability to deserialize from ReadOnlyMemory<byte> ([558d998](https://github.com/Elders/Cronus/commit/558d9988af9134e0f8684885f3480dac389b4618))
* Rework of rebuild job ([c68a013](https://github.com/Elders/Cronus/commit/c68a01312836794f777c3457186421539ad90fad))

# [7.0.0-preview.40](https://github.com/Elders/Cronus/compare/v7.0.0-preview.39...v7.0.0-preview.40) (2022-03-31)


### Bug Fixes

* update Domain Modeling package ([b994800](https://github.com/Elders/Cronus/commit/b9948006f2db568c894c03f35b17d931a12a3916))

# [7.0.0-preview.39](https://github.com/Elders/Cronus/compare/v7.0.0-preview.38...v7.0.0-preview.39) (2022-03-18)


### Bug Fixes

* Fix build ([a2f325c](https://github.com/Elders/Cronus/commit/a2f325c4e60212afb4714c6eed45257b59a8f08a))

# [7.0.0-preview.38](https://github.com/Elders/Cronus/compare/v7.0.0-preview.37...v7.0.0-preview.38) (2022-03-16)


### Bug Fixes

* Fix error handling in async lamda + fix while loop condition ([e30b468](https://github.com/Elders/Cronus/commit/e30b46830c4c40fe9fb15de383cf0ecc207eec7a))

# [7.0.0-preview.37](https://github.com/Elders/Cronus/compare/v7.0.0-preview.36...v7.0.0-preview.37) (2022-03-14)


### Features

* Rework of rebuild job ([c68a013](https://github.com/Elders/Cronus/commit/c68a01312836794f777c3457186421539ad90fad))

# [7.0.0-preview.36](https://github.com/Elders/Cronus/compare/v7.0.0-preview.35...v7.0.0-preview.36) (2022-03-11)


### Bug Fixes

* Rolls back how projections are rebuilt ([f02a065](https://github.com/Elders/Cronus/commit/f02a065c611d03e1368f59c36f03135f64c3a2d8))

# [7.0.0-preview.35](https://github.com/Elders/Cronus/compare/v7.0.0-preview.34...v7.0.0-preview.35) (2022-03-09)


### Bug Fixes

* Revert "fix: Makes the the AR id value format in the message header consistent everywhere" ([2136fbc](https://github.com/Elders/Cronus/commit/2136fbcb9e25c93ec486c8b0aaf8d5ec14af4fb4))

# [7.0.0-preview.34](https://github.com/Elders/Cronus/compare/v7.0.0-preview.33...v7.0.0-preview.34) (2022-03-07)


### Bug Fixes

* Accelerate replay/rebuild job ([ae02d19](https://github.com/Elders/Cronus/commit/ae02d19139cc3662172eb66af6360607c438459a))

# [7.0.0-preview.33](https://github.com/Elders/Cronus/compare/v7.0.0-preview.32...v7.0.0-preview.33) (2022-03-03)


### Bug Fixes

* Removes retries ([ab489d2](https://github.com/Elders/Cronus/commit/ab489d2816ef91bf094c058ea0a9ea02b9ce0f42))

# [7.0.0-preview.32](https://github.com/Elders/Cronus/compare/v7.0.0-preview.31...v7.0.0-preview.32) (2022-03-02)


### Bug Fixes

* And the other eye ([c98b74c](https://github.com/Elders/Cronus/commit/c98b74c4cbfb822079183dea7ede3c00ef746b64))
* My eyes ([d9eb59d](https://github.com/Elders/Cronus/commit/d9eb59df07e706646170b793b9834a56e0875dfe))

# [7.0.0-preview.31](https://github.com/Elders/Cronus/compare/v7.0.0-preview.30...v7.0.0-preview.31) (2022-03-02)


### Bug Fixes

* Adds retries and log when replaying projection ([f5ebdff](https://github.com/Elders/Cronus/commit/f5ebdff614924677b4cb7db2a927aa94a6310667))

# [7.0.0-preview.30](https://github.com/Elders/Cronus/compare/v7.0.0-preview.29...v7.0.0-preview.30) (2022-03-02)


### Bug Fixes

* Configures the signal settings ([49cef65](https://github.com/Elders/Cronus/commit/49cef65603aed8840c115b84b0b748a139d43a31))

# [7.0.0-preview.29](https://github.com/Elders/Cronus/compare/v7.0.0-preview.28...v7.0.0-preview.29) (2022-02-24)


### Bug Fixes

* Change subscriber workflow for Triggers ([9e92912](https://github.com/Elders/Cronus/commit/9e929121edeb647cef151e247438bd6645b529ca))
* Introducing live progress with fast signals ([cbf4704](https://github.com/Elders/Cronus/commit/cbf4704fc030a3ab2b98986e4dad5a7cb2318f26))
* Launch fast signals ([3b8d505](https://github.com/Elders/Cronus/commit/3b8d5052b2e6253081b6d3127813882d1a625d32))

# [7.0.0-preview.28](https://github.com/Elders/Cronus/compare/v7.0.0-preview.27...v7.0.0-preview.28) (2022-02-15)


### Bug Fixes

* Makes the the AR id value format in the message header consistent everywhere ([daf2f31](https://github.com/Elders/Cronus/commit/daf2f312ed6d725c618afbc74d101272392c1a61))

# [7.0.0-preview.27](https://github.com/Elders/Cronus/compare/v7.0.0-preview.26...v7.0.0-preview.27) (2022-02-09)


### Bug Fixes

* Update Domain Modeling package version ([17ab387](https://github.com/Elders/Cronus/commit/17ab387a6510dc803d95e309eca8da0c7dc2fb37))

# [7.0.0-preview.26](https://github.com/Elders/Cronus/compare/v7.0.0-preview.25...v7.0.0-preview.26) (2022-02-09)


### Bug Fixes

* Update Domain Modeling package version ([3ac9b1c](https://github.com/Elders/Cronus/commit/3ac9b1c5f2f39533ec14afb16382e1daed8b8dea))

# [7.0.0-preview.25](https://github.com/Elders/Cronus/compare/v7.0.0-preview.24...v7.0.0-preview.25) (2022-02-08)


### Bug Fixes

* Log unsuccessful publish ([01fc3a9](https://github.com/Elders/Cronus/commit/01fc3a9eb7dbb4db1a22f4cb1250e2093fc1ea17))

# [7.0.0-preview.24](https://github.com/Elders/Cronus/compare/v7.0.0-preview.23...v7.0.0-preview.24) (2022-02-08)


### Bug Fixes

* Sending heartbeat twice, since now every service should use AddCronusHeartbeat ([604058f](https://github.com/Elders/Cronus/commit/604058f7bba5c63148072122e9cd67e621ec8f1c))

# [7.0.0-preview.23](https://github.com/Elders/Cronus/compare/v7.0.0-preview.22...v7.0.0-preview.23) (2022-02-07)


### Bug Fixes

* Add TTL to MessageHeaders ([00074c6](https://github.com/Elders/Cronus/commit/00074c61b4547c7691d132138b5e7f5c2adf0948))

# [7.0.0-preview.22](https://github.com/Elders/Cronus/compare/v7.0.0-preview.21...v7.0.0-preview.22) (2022-02-03)


### Bug Fixes

* Build fix ([175640e](https://github.com/Elders/Cronus/commit/175640ec6d8e7a0d505abf23a063724df214260e))


### Features

* add EnvironmentConfig field ([a2357c7](https://github.com/Elders/Cronus/commit/a2357c731357eb095c29492017d816b10d51c7f3))

# [7.0.0-preview.21](https://github.com/Elders/Cronus/compare/v7.0.0-preview.20...v7.0.0-preview.21) (2022-02-02)


### Bug Fixes

* Add heartbeat interval to settigns ([a09cd3f](https://github.com/Elders/Cronus/commit/a09cd3f13733fb3a0a69553774151df375f4db1a))

# [7.0.0-preview.20](https://github.com/Elders/Cronus/compare/v7.0.0-preview.19...v7.0.0-preview.20) (2022-02-01)


### Bug Fixes

* Add Machine name to heartbeat ([49ae370](https://github.com/Elders/Cronus/commit/49ae3707acf8f438778b4db38baa3798d17b9602))

# [7.0.0-preview.19](https://github.com/Elders/Cronus/compare/v7.0.0-preview.18...v7.0.0-preview.19) (2022-02-01)


### Bug Fixes

* Moved to synchronous Start ([54e80f9](https://github.com/Elders/Cronus/commit/54e80f95698505d5d0ca71b6aacace857e5ea210))

# [7.0.0-preview.18](https://github.com/Elders/Cronus/compare/v7.0.0-preview.17...v7.0.0-preview.18) (2022-01-27)


### Features

* Adds heartbeat ([60f703e](https://github.com/Elders/Cronus/commit/60f703e4a75fb01cfea8b2193127219dc173b325))

# [7.0.0-preview.17](https://github.com/Elders/Cronus/compare/v7.0.0-preview.16...v7.0.0-preview.17) (2022-01-26)


### Bug Fixes

* Bootstrap Cronus before all services have started ([07d686d](https://github.com/Elders/Cronus/commit/07d686d8edef56ae52aefdc81ab7bf5f94c92691))

# [7.0.0-preview.16](https://github.com/Elders/Cronus/compare/v7.0.0-preview.15...v7.0.0-preview.16) (2022-01-21)


### Bug Fixes

* Adds try catches on all job runner methods ([41d42e5](https://github.com/Elders/Cronus/commit/41d42e5711926b96b26e0a1e68d72d602638ee24))

# [7.0.0-preview.15](https://github.com/Elders/Cronus/compare/v7.0.0-preview.14...v7.0.0-preview.15) (2022-01-18)


### Bug Fixes

* Ensure service provider in ErrorContext ([24af784](https://github.com/Elders/Cronus/commit/24af78470a4af3baa515f6fd12412986b93a2661))

# [7.0.0-preview.14](https://github.com/Elders/Cronus/compare/v7.0.0-preview.13...v7.0.0-preview.14) (2022-01-18)


### Bug Fixes

* Add logging om handle error ([634e6ba](https://github.com/Elders/Cronus/commit/634e6ba2e7a0059d67a60e7775d4dd52f83e592d))

# [7.0.0-preview.13](https://github.com/Elders/Cronus/compare/v7.0.0-preview.12...v7.0.0-preview.13) (2022-01-18)


### Features

* Adds the ability to deserialize from ReadOnlyMemory<byte> ([558d998](https://github.com/Elders/Cronus/commit/558d9988af9134e0f8684885f3480dac389b4618))

# [7.0.0-preview.12](https://github.com/Elders/Cronus/compare/v7.0.0-preview.11...v7.0.0-preview.12) (2022-01-18)


### Bug Fixes

* Change  NotFound projection message log level to Debug ([5c18665](https://github.com/Elders/Cronus/commit/5c186659c965682eb3c038634476c7443de97196))

# [7.0.0-preview.11](https://github.com/Elders/Cronus/compare/v7.0.0-preview.10...v7.0.0-preview.11) (2022-01-13)


### Bug Fixes

* Reverts the diagnostic logs because they create a log of noise at the moment ([386c224](https://github.com/Elders/Cronus/commit/386c224be56984917e9aada2f1ac68778df66345))

# [7.0.0-preview.10](https://github.com/Elders/Cronus/compare/v7.0.0-preview.9...v7.0.0-preview.10) (2022-01-13)


### Bug Fixes

* bump version ([507111c](https://github.com/Elders/Cronus/commit/507111c73c5f7a388ceadca9ec11165f4fd81181))

# [7.0.0-preview.9](https://github.com/Elders/Cronus/compare/v7.0.0-preview.8...v7.0.0-preview.9) (2021-12-20)


### Bug Fixes

* Fixes projection version building ([17e417e](https://github.com/Elders/Cronus/commit/17e417ecd5eda5426cf04799c43a7eec34c92f29))

# [7.0.0-preview.8](https://github.com/Elders/Cronus/compare/v7.0.0-preview.7...v7.0.0-preview.8) (2021-12-17)


### Bug Fixes

* Fixes projection version handler not loading ([70b975e](https://github.com/Elders/Cronus/commit/70b975ec05ec0a577c909ecbdbce97cf6ca18080))

# [7.0.0-preview.7](https://github.com/Elders/Cronus/compare/v7.0.0-preview.6...v7.0.0-preview.7) (2021-12-16)


### Bug Fixes

* log elapsed time for indexing AR commits ([5c630c5](https://github.com/Elders/Cronus/commit/5c630c5b4846371ccec5c427eca23ce3d02f3f65))

# [7.0.0-preview.6](https://github.com/Elders/Cronus/compare/v7.0.0-preview.5...v7.0.0-preview.6) (2021-12-14)


### Bug Fixes

* Add timered logger for every Rebuild index operation [INT] ([fccd98c](https://github.com/Elders/Cronus/commit/fccd98c6628b08454d8864c22f60a8d0618e1e28))

# [7.0.0-preview.5](https://github.com/Elders/Cronus/compare/v7.0.0-preview.4...v7.0.0-preview.5) (2021-12-14)


### Features

* Add snapshot skipping optimization for not snapshotable projections ([5d2771b](https://github.com/Elders/Cronus/commit/5d2771b8fc45d7bc375bde3672dba67faaa05f7e))

# [7.0.0-preview.4](https://github.com/Elders/Cronus/compare/v7.0.0-preview.3...v7.0.0-preview.4) (2021-12-02)


### Bug Fixes

* Fixes Linux paths. _!_0x M$ ([e76ea49](https://github.com/Elders/Cronus/commit/e76ea490262243b5f15d7c9faba0f0c643548f5f))

# [7.0.0-preview.3](https://github.com/Elders/Cronus/compare/v7.0.0-preview.2...v7.0.0-preview.3) (2021-11-30)


### Bug Fixes

* Add Aggregate Commit transformation interface ([045c617](https://github.com/Elders/Cronus/commit/045c617ccc11e4239c156b06c6929a6fda75029c))
* Add interceptor for modifying upcomming aggregate commits ([55ba3f9](https://github.com/Elders/Cronus/commit/55ba3f9abd8b12639d09fee012d815234080f780))

# [7.0.0-preview.2](https://github.com/Elders/Cronus/compare/v7.0.0-preview.1...v7.0.0-preview.2) (2021-11-15)


### Bug Fixes

* Removes obsolete log extensions ([5449e27](https://github.com/Elders/Cronus/commit/5449e27f88775efd74ddb45ff83cb2a9a3d3e166))
* Transitions to Assembly.Location from Assembly.CodeBase ([b6b4a94](https://github.com/Elders/Cronus/commit/b6b4a948c7a10ee6ccb997b8bfa2ed184d93045d))

# [7.0.0-preview.1](https://github.com/Elders/Cronus/compare/v6.4.3...v7.0.0-preview.1) (2021-11-11)

## [6.4.3](https://github.com/Elders/Cronus/compare/v6.4.2...v6.4.3) (2021-11-05)


### Bug Fixes

* bump Cronus.DomainModeling from 6.3.3 to 6.3.4 ([0b11c66](https://github.com/Elders/Cronus/commit/0b11c66f05e150bdc7669057bd397e506b5362d1))

## [6.4.2](https://github.com/Elders/Cronus/compare/v6.4.1...v6.4.2) (2021-11-05)


### Bug Fixes

* Change dependabot configuration ([1a3537f](https://github.com/Elders/Cronus/commit/1a3537f2df8f28ae482050125de42e9a8a1d0d40))

## [6.4.1](https://github.com/Elders/Cronus/compare/v6.4.0...v6.4.1) (2021-11-05)


### Bug Fixes

* When line position matters ([28f8f87](https://github.com/Elders/Cronus/commit/28f8f8718f2c64cd229919090826fd7538d2dd6e))

# [6.4.0](https://github.com/Elders/Cronus/compare/v6.3.0...v6.4.0) (2021-11-05)


### Bug Fixes

* Adds a new method to load aggregate commits ([a6e055f](https://github.com/Elders/Cronus/commit/a6e055f4bdaf4291f469687069fd383f447f7dfb))
* Adds a parameter check when resolving a tenant ([088bb20](https://github.com/Elders/Cronus/commit/088bb20b7aa19d6894316d8c4cdfaad2ed14b246))
* Adds additional logs to the job ([16c24b4](https://github.com/Elders/Cronus/commit/16c24b4322397191ec2c2ceb1419f252ace821da))
* Adds an async version of the HasSnapshotMarkerAsync ([e2e0520](https://github.com/Elders/Cronus/commit/e2e052054847385759cdc9219dcb7a3054eacab9))
* Adds beta branch to the azure-pipelines ([f44e9d4](https://github.com/Elders/Cronus/commit/f44e9d4771b8fca79fa2cbed7c9c3f844a9a0ac3))
* Adds DataContract attribute to GGPort handler ([8990991](https://github.com/Elders/Cronus/commit/8990991444281b38ea8a15e81d9ae9f2e8301a47))
* Adds more detailed logging information in Publisher ([4c6761d](https://github.com/Elders/Cronus/commit/4c6761d46773c03bdb54e12855727456ec06ce13))
* Adds public events replay functionality. This is a POC so it will be changed in future ([9d03d6d](https://github.com/Elders/Cronus/commit/9d03d6d84c8c56bb03a8827d688e4337c76f7fae))
* Adds system services options ([9fb6d09](https://github.com/Elders/Cronus/commit/9fb6d09a0ad076216370f75c0ac7df7b3197f59d))
* Allows custom logic to be injected on aggregate commit during a migration ([334ffe1](https://github.com/Elders/Cronus/commit/334ffe1586dc2adc32c09ce1f6ed740a04b8ab25))
* change contract id on projection version handler ([6f81e6d](https://github.com/Elders/Cronus/commit/6f81e6d896cb57c317ff51b4dcb7e998defa76e5))
* Clean up ([c1bdf4e](https://github.com/Elders/Cronus/commit/c1bdf4eadac47934f42e88ffce6c66a42071cec3))
* Code cleanup ([68363f7](https://github.com/Elders/Cronus/commit/68363f762e72137f5d4b1770ec30347ed1547ec5))
* Code cleanup ([527b1d4](https://github.com/Elders/Cronus/commit/527b1d401b58f7ac0d0e24cc0e0d68e7393d9624))
* Code cleanup ([5267a05](https://github.com/Elders/Cronus/commit/5267a052cebdbb52705a932b792554ed2d0ebc91))
* Enrich logs on projection rebuild ([b22cd3f](https://github.com/Elders/Cronus/commit/b22cd3f451cd4a36fd6ada3c69cb84cf0daf5f00))
* Fix bug where projectionversionhandler could not be rebuilt ([333e3e6](https://github.com/Elders/Cronus/commit/333e3e6ae6877a279c4b7d522016077bfcbea3da))
* Fix bug where stale versions were not removed ([6089576](https://github.com/Elders/Cronus/commit/6089576cc4aa509f9a401a6ff70b039282840a83))
* fix issue where stale Rebuilding version are not timedout ([e8aa75c](https://github.com/Elders/Cronus/commit/e8aa75ca4f43bd2cffb7ffb7b089f7ac49dce76d))
* fix registration of event store and tests ([0ffae2e](https://github.com/Elders/Cronus/commit/0ffae2e44837151011c107ed2eaff2d6c5b23cdc))
* Fix republish for IEventSourced projections ([b3068ae](https://github.com/Elders/Cronus/commit/b3068ae704f25f8a6c9993891d03dbe0290f0f3e))
* Fix version not being able to be canceled or timedout ([edafa6c](https://github.com/Elders/Cronus/commit/edafa6c163b57c77be8026f99648a48f185b4043))
* Fix versions not being able to be Canceled ([b71bbf6](https://github.com/Elders/Cronus/commit/b71bbf6094eca7cd1d711ea6388e624fb58765a6))
* Fixes copyright ([8c0b628](https://github.com/Elders/Cronus/commit/8c0b628b2f75522bacd58655d18e8a1508b79239))
* Fixes copyright date ([c2bd0bf](https://github.com/Elders/Cronus/commit/c2bd0bf23858f3ff2370da36b9e6b0929a8686dd))
* Fixes projection snapshot revisions ([d1292bc](https://github.com/Elders/Cronus/commit/d1292bc01a771f4dcd777a014a896600d78cd69b))
* Fixes release.config branch ([30e9b92](https://github.com/Elders/Cronus/commit/30e9b922e1c162fc3f73636a4bd5d66382ca7091))
* fixes release.config for preview branch ([d7d7794](https://github.com/Elders/Cronus/commit/d7d779474b7ba66e3ddf3aab201a133fb095736e))
* Handles better projection building when aggregate commit cant be deserialized ([3f783d4](https://github.com/Elders/Cronus/commit/3f783d42d8c13bcb68a2422e8a39fd676fab0cce))
* Improves how the projections are rebuilt ([82be859](https://github.com/Elders/Cronus/commit/82be859ef5d564caae809d1c9618a698176cecdb))
* Improves jobs logging ([4389689](https://github.com/Elders/Cronus/commit/43896898ecc24a9645c917862fc2736a617e18a7))
* Improves logging when a projection instance fails to load ([790a668](https://github.com/Elders/Cronus/commit/790a6682e56ad23c0b83163178e30e6ac8e779f3))
* Improves logging when a projection instance is not found ([f44853f](https://github.com/Elders/Cronus/commit/f44853fb7f282577d821aa638af4381a25e59a91))
* Index public events ([5fdb6a7](https://github.com/Elders/Cronus/commit/5fdb6a74841ed1cdf5b287d5722fe8776c07d558))
* Properly creates AR for index ([7cdab04](https://github.com/Elders/Cronus/commit/7cdab04637da98d83f3944d4e0d399806c6f1625))
* Properly logs projectionid via structured logging ([8c30dd3](https://github.com/Elders/Cronus/commit/8c30dd33ce247057cc1e30d777701a1a7ef1fe89))
* Properly re-publishes public events ([bc7f9f9](https://github.com/Elders/Cronus/commit/bc7f9f9a8032cb5a3032830651d148ef314aa220))
* Remove recursive behavior ( Stack Overflow ) ([f1a8968](https://github.com/Elders/Cronus/commit/f1a89685da97f36bf1c5ebe4a0bd3456693d63ec))
* Removes old release notes file ([c2f8ef3](https://github.com/Elders/Cronus/commit/c2f8ef3b16e880c1849a9df11369032ae0b008f1))
* Removes unused contract. sorry ([d54ddc1](https://github.com/Elders/Cronus/commit/d54ddc1627922f0cb56b56c2f5e99a51be5dec45))
* Reuses the projection index code ([ee5bb37](https://github.com/Elders/Cronus/commit/ee5bb3774abb98f0e0b52d1d69aca923e3b68fbb))
* Revert change contract id on projection version handler ([94fbf4d](https://github.com/Elders/Cronus/commit/94fbf4d0ea1972e06777edef3989bcdd5bbca2ad))
* Separates system messages and handlers ([d8f0b25](https://github.com/Elders/Cronus/commit/d8f0b2599af0c574f80149493748e21d5bbe7037))
* Stops starting indexes if migrations are enabled ([1f31fdf](https://github.com/Elders/Cronus/commit/1f31fdf48f90db591174ee8ab739a63161549c6f))
* Updates DomainModeling ([2b2575f](https://github.com/Elders/Cronus/commit/2b2575f3cc9589b1935a8f57d5151da0fbce4686))
* Updates DomainModeling ([dfeb36a](https://github.com/Elders/Cronus/commit/dfeb36a47cd7594d94cba07abc9d885d30888c2e))
* Updates DomainModeling ([85b318b](https://github.com/Elders/Cronus/commit/85b318bec833817463364eaf6aeb978d02ef3517))


### Features

* Adds aggregate commit publishing support ([05eb094](https://github.com/Elders/Cronus/commit/05eb094b58391beed08dab38aea1f07d5154b604))
* Release ([442a16c](https://github.com/Elders/Cronus/commit/442a16c70fae87640141e59c4e41743ef3c380af))
* removes snapshots ([2010c89](https://github.com/Elders/Cronus/commit/2010c8997176cac4b7f7f1f0bf2cde724d45b5e9))

# [6.4.0-preview.24](https://github.com/Elders/Cronus/compare/v6.4.0-preview.23...v6.4.0-preview.24) (2021-11-03)


### Bug Fixes

* Adds more detailed logging information in Publisher ([194eea3](https://github.com/Elders/Cronus/commit/194eea38e8d6e031a8b6213e5edf0f53900e3520))

# [6.4.0-preview.23](https://github.com/Elders/Cronus/compare/v6.4.0-preview.22...v6.4.0-preview.23) (2021-10-21)


### Bug Fixes

* Properly re-publishes public events ([47cf7eb](https://github.com/Elders/Cronus/commit/47cf7eb29133f118f59be430e0932087390d686d))

# [6.4.0-preview.22](https://github.com/Elders/Cronus/compare/v6.4.0-preview.21...v6.4.0-preview.22) (2021-10-18)


### Bug Fixes

* Fix bug where projectionversionhandler could not be rebuilt ([144e73d](https://github.com/Elders/Cronus/commit/144e73d16a60e0fece62bf172a1c6df42ef41748))

# [6.4.0-preview.21](https://github.com/Elders/Cronus/compare/v6.4.0-preview.20...v6.4.0-preview.21) (2021-10-18)


### Bug Fixes

* Enrich logs on projection rebuild ([5d93a8c](https://github.com/Elders/Cronus/commit/5d93a8c4e109cebf728c9e3bcd7295118c36150a))

# [6.4.0-preview.20](https://github.com/Elders/Cronus/compare/v6.4.0-preview.19...v6.4.0-preview.20) (2021-10-14)


### Bug Fixes

* Fix versions not being able to be Canceled ([8dbd5a6](https://github.com/Elders/Cronus/commit/8dbd5a6143751f1145bb556b0afbfb4bcea580cc))

# [6.4.0-preview.19](https://github.com/Elders/Cronus/compare/v6.4.0-preview.18...v6.4.0-preview.19) (2021-10-13)


### Bug Fixes

* fix registration of event store and tests ([dd6a632](https://github.com/Elders/Cronus/commit/dd6a632192d97a53bf6e248e207d4e8d90649616))
* Handles better projection building when aggregate commit cant be deserialized ([7733ca6](https://github.com/Elders/Cronus/commit/7733ca64e073cc593d968c1682b638f1a2a2b4d5))

# [6.4.0-preview.18](https://github.com/Elders/Cronus/compare/v6.4.0-preview.17...v6.4.0-preview.18) (2021-09-14)


### Bug Fixes

* fix issue where stale Rebuilding version are not timedout ([5e9cb7b](https://github.com/Elders/Cronus/commit/5e9cb7bb4b3c7710b514b5a6469181454e6a1a4e))

# [6.4.0-preview.17](https://github.com/Elders/Cronus/compare/v6.4.0-preview.16...v6.4.0-preview.17) (2021-09-14)


### Bug Fixes

* Fix version not being able to be canceled or timedout ([ae227b0](https://github.com/Elders/Cronus/commit/ae227b097f78439d82d55a6cbea615e3ece17289))

# [6.4.0-preview.16](https://github.com/Elders/Cronus/compare/v6.4.0-preview.15...v6.4.0-preview.16) (2021-09-14)


### Bug Fixes

* Remove recursive behavior ( Stack Overflow ) ([3f1de2d](https://github.com/Elders/Cronus/commit/3f1de2d10bc9045a4282492c476e8b4dc672c10c))

# [6.4.0-preview.15](https://github.com/Elders/Cronus/compare/v6.4.0-preview.14...v6.4.0-preview.15) (2021-08-05)


### Bug Fixes

* Clean up ([b260caf](https://github.com/Elders/Cronus/commit/b260caf37cce8c519c978a97ab96194f9f64167e))

# [6.4.0-preview.14](https://github.com/Elders/Cronus/compare/v6.4.0-preview.13...v6.4.0-preview.14) (2021-08-04)


### Bug Fixes

* Revert change contract id on projection version handler ([31a41a9](https://github.com/Elders/Cronus/commit/31a41a9603e5fa671a9385a149cd7e43d1b58cb9))

# [6.4.0-preview.13](https://github.com/Elders/Cronus/compare/v6.4.0-preview.12...v6.4.0-preview.13) (2021-08-03)


### Bug Fixes

* Fix bug where stale versions were not removed ([e872fca](https://github.com/Elders/Cronus/commit/e872fcacd151162abe83b23f97485f9ede2095d5))

# [6.4.0-preview.12](https://github.com/Elders/Cronus/compare/v6.4.0-preview.11...v6.4.0-preview.12) (2021-08-02)


### Bug Fixes

* change contract id on projection version handler ([4c50aa9](https://github.com/Elders/Cronus/commit/4c50aa9e659b7e68db556c4bd4e688f3caa37266))

# [6.4.0-preview.11](https://github.com/Elders/Cronus/compare/v6.4.0-preview.10...v6.4.0-preview.11) (2021-07-21)


### Bug Fixes

* Code cleanup ([fd1ceb4](https://github.com/Elders/Cronus/commit/fd1ceb4c2dcbf1e7727671427a3cacfb95141edc))

# [6.4.0-preview.10](https://github.com/Elders/Cronus/compare/v6.4.0-preview.9...v6.4.0-preview.10) (2021-07-06)


### Bug Fixes

* Fix republish for IEventSourced projections ([48b7e8d](https://github.com/Elders/Cronus/commit/48b7e8d2bd6a589a3191cfae1b4e5b94921ff257))

# [6.4.0-preview.9](https://github.com/Elders/Cronus/compare/v6.4.0-preview.8...v6.4.0-preview.9) (2021-06-28)


### Bug Fixes

* Adds a parameter check when resolving a tenant ([665d064](https://github.com/Elders/Cronus/commit/665d064327f206d0aa9143949d7faaab7d80c198))
* Improves how the projections are rebuilt ([7d5cd8c](https://github.com/Elders/Cronus/commit/7d5cd8c4d6897159e03ea618002f0914f0db005a))

# [6.4.0-preview.8](https://github.com/Elders/Cronus/compare/v6.4.0-preview.7...v6.4.0-preview.8) (2021-05-28)


### Bug Fixes

* Index public events ([4dc5a33](https://github.com/Elders/Cronus/commit/4dc5a33e88b87a4ce204369427ea061f0bde5ab7))

# [6.4.0-preview.7](https://github.com/Elders/Cronus/compare/v6.4.0-preview.6...v6.4.0-preview.7) (2021-05-24)


### Bug Fixes

* Properly logs projectionid via structured logging ([a582fdf](https://github.com/Elders/Cronus/commit/a582fdf3a4b0fbb269298a500608485cbb0975ba))

# [6.4.0-preview.6](https://github.com/Elders/Cronus/compare/v6.4.0-preview.5...v6.4.0-preview.6) (2021-05-12)


### Bug Fixes

* Improves logging when a projection instance fails to load ([b0efe61](https://github.com/Elders/Cronus/commit/b0efe61d18fa9a1dd87e0ae16c9f12d952f3c311))

# [6.4.0-preview.5](https://github.com/Elders/Cronus/compare/v6.4.0-preview.4...v6.4.0-preview.5) (2021-05-12)


### Bug Fixes

* Improves logging when a projection instance is not found ([7f2177a](https://github.com/Elders/Cronus/commit/7f2177a6d9fabb8d79d5c91f7bd925caeef2ec8d))

# [6.4.0-preview.4](https://github.com/Elders/Cronus/compare/v6.4.0-preview.3...v6.4.0-preview.4) (2021-05-11)


### Bug Fixes

* Adds a new method to load aggregate commits ([800d819](https://github.com/Elders/Cronus/commit/800d819d3d96b44b53b02517f58c72e85273fb2b))
* Adds DataContract attribute to GGPort handler ([8774b7d](https://github.com/Elders/Cronus/commit/8774b7d6aadbfd6b2b4826ce9c9d2096d3b00cc4))
* Adds public events replay functionality. This is a POC so it will be changed in future ([4636716](https://github.com/Elders/Cronus/commit/4636716c2b6acffb476012e54bc43b603c7042cd))

# [6.4.0-preview.3](https://github.com/Elders/Cronus/compare/v6.4.0-preview.2...v6.4.0-preview.3) (2021-05-05)


### Bug Fixes

* Updates DomainModeling ([33b9895](https://github.com/Elders/Cronus/commit/33b98953a53a588603ae733ab7841c907b2211c9))

# [6.4.0-preview.2](https://github.com/Elders/Cronus/compare/v6.4.0-preview.1...v6.4.0-preview.2) (2021-05-05)


### Bug Fixes

* Updates DomainModeling ([6b14b18](https://github.com/Elders/Cronus/commit/6b14b186a42747c8c6deaea110dab3695bb3cad7))

# [6.4.0-preview.1](https://github.com/Elders/Cronus/compare/v6.4.0-next.13...v6.4.0-preview.1) (2021-05-05)


### Bug Fixes

* Adds beta branch to the azure-pipelines ([fc0e227](https://github.com/Elders/Cronus/commit/fc0e2277d2cd8684e9dd4191b50a766a3113e179))
* Fixes copyright ([f56c5c5](https://github.com/Elders/Cronus/commit/f56c5c575a197f02f121bba763acb7faf9863c35))
* Fixes copyright date ([2a78e58](https://github.com/Elders/Cronus/commit/2a78e589c9833e6b8621b5b7c169e5a3312999a8))
* Fixes release.config branch ([3224128](https://github.com/Elders/Cronus/commit/32241289f04a571f45fdb9e7d605d6e8f23ffd54))
* fixes release.config for preview branch ([59fc4ca](https://github.com/Elders/Cronus/commit/59fc4caf8d60f8557bf1f22435dc65a80cead6e3))
* Properly creates AR for index ([89f29d7](https://github.com/Elders/Cronus/commit/89f29d7abf084bf4f6076abb24a9339065f7ed4f))
* Removes old release notes file ([6c37880](https://github.com/Elders/Cronus/commit/6c3788002d876ecaba5617b7eedee8e16ce54099))
* Updates DomainModeling ([16bf88b](https://github.com/Elders/Cronus/commit/16bf88bee60f9db76b98e6b8aaa17e83d1949d88))

# [6.4.0-next.14](https://github.com/Elders/Cronus/compare/v6.4.0-next.13...v6.4.0-next.14) (2021-05-05)


### Bug Fixes

* Adds beta branch to the azure-pipelines ([fc0e227](https://github.com/Elders/Cronus/commit/fc0e2277d2cd8684e9dd4191b50a766a3113e179))
* Fixes copyright ([f56c5c5](https://github.com/Elders/Cronus/commit/f56c5c575a197f02f121bba763acb7faf9863c35))
* Fixes copyright date ([2a78e58](https://github.com/Elders/Cronus/commit/2a78e589c9833e6b8621b5b7c169e5a3312999a8))
* Fixes release.config branch ([3224128](https://github.com/Elders/Cronus/commit/32241289f04a571f45fdb9e7d605d6e8f23ffd54))
* Properly creates AR for index ([89f29d7](https://github.com/Elders/Cronus/commit/89f29d7abf084bf4f6076abb24a9339065f7ed4f))
* Updates DomainModeling ([16bf88b](https://github.com/Elders/Cronus/commit/16bf88bee60f9db76b98e6b8aaa17e83d1949d88))

# [6.4.0-next.14](https://github.com/Elders/Cronus/compare/v6.4.0-next.13...v6.4.0-next.14) (2021-05-05)


### Bug Fixes

* Adds beta branch to the azure-pipelines ([fc0e227](https://github.com/Elders/Cronus/commit/fc0e2277d2cd8684e9dd4191b50a766a3113e179))
* Fixes copyright ([f56c5c5](https://github.com/Elders/Cronus/commit/f56c5c575a197f02f121bba763acb7faf9863c35))
* Fixes copyright date ([2a78e58](https://github.com/Elders/Cronus/commit/2a78e589c9833e6b8621b5b7c169e5a3312999a8))
* Fixes release.config branch ([3224128](https://github.com/Elders/Cronus/commit/32241289f04a571f45fdb9e7d605d6e8f23ffd54))
* Properly creates AR for index ([89f29d7](https://github.com/Elders/Cronus/commit/89f29d7abf084bf4f6076abb24a9339065f7ed4f))
* Updates DomainModeling ([16bf88b](https://github.com/Elders/Cronus/commit/16bf88bee60f9db76b98e6b8aaa17e83d1949d88))

# [6.4.0-next.14](https://github.com/Elders/Cronus/compare/v6.4.0-next.13...v6.4.0-next.14) (2021-05-05)


### Bug Fixes

* Adds beta branch to the azure-pipelines ([fc0e227](https://github.com/Elders/Cronus/commit/fc0e2277d2cd8684e9dd4191b50a766a3113e179))
* Fixes copyright ([f56c5c5](https://github.com/Elders/Cronus/commit/f56c5c575a197f02f121bba763acb7faf9863c35))
* Fixes copyright date ([2a78e58](https://github.com/Elders/Cronus/commit/2a78e589c9833e6b8621b5b7c169e5a3312999a8))
* Fixes release.config branch ([3224128](https://github.com/Elders/Cronus/commit/32241289f04a571f45fdb9e7d605d6e8f23ffd54))
* Properly creates AR for index ([89f29d7](https://github.com/Elders/Cronus/commit/89f29d7abf084bf4f6076abb24a9339065f7ed4f))
* Updates DomainModeling ([16bf88b](https://github.com/Elders/Cronus/commit/16bf88bee60f9db76b98e6b8aaa17e83d1949d88))

# [6.4.0-next.13](https://github.com/Elders/Cronus/compare/v6.4.0-next.12...v6.4.0-next.13) (2021-04-12)


### Bug Fixes

* Code cleanup ([871e806](https://github.com/Elders/Cronus/commit/871e8066ccaa98ffe0f68c965a027c30fb733c07))

# [6.4.0-next.12](https://github.com/Elders/Cronus/compare/v6.4.0-next.11...v6.4.0-next.12) (2021-04-09)


### Bug Fixes

* Allows custom logic to be injected on aggregate commit during a migration ([063a036](https://github.com/Elders/Cronus/commit/063a036520df8124bd910ef392b49d72c586a1e9))

# [6.4.0-next.11](https://github.com/Elders/Cronus/compare/v6.4.0-next.10...v6.4.0-next.11) (2021-03-31)


### Bug Fixes

* Adds system services options ([9f9bc49](https://github.com/Elders/Cronus/commit/9f9bc49ed9851ccfeeba1e9336dd1d72a49e4e74))

# [6.4.0-next.10](https://github.com/Elders/Cronus/compare/v6.4.0-next.9...v6.4.0-next.10) (2021-03-31)


### Bug Fixes

* Separates system messages and handlers ([cafd92a](https://github.com/Elders/Cronus/commit/cafd92a0e508e403669073f41b76d84a84cc04fc))

# [6.4.0-next.9](https://github.com/Elders/Cronus/compare/v6.4.0-next.8...v6.4.0-next.9) (2021-03-30)


### Bug Fixes

* Stops starting indexes if migrations are enabled ([8df11fb](https://github.com/Elders/Cronus/commit/8df11fb0243d757e450388d48db0ef25fcd9d859))

# [6.4.0-next.8](https://github.com/Elders/Cronus/compare/v6.4.0-next.7...v6.4.0-next.8) (2021-03-29)


### Features

* Adds aggregate commit publishing support ([2d7cda4](https://github.com/Elders/Cronus/commit/2d7cda4e507f5cfbb619dbf7354131802f55dfc5))

# [6.4.0-next.7](https://github.com/Elders/Cronus/compare/v6.4.0-next.6...v6.4.0-next.7) (2021-03-28)


### Bug Fixes

* Adds additional logs to the job ([cbf1e64](https://github.com/Elders/Cronus/commit/cbf1e642d88e56679dea16c783584cf3d430e19a))

# [6.4.0-next.6](https://github.com/Elders/Cronus/compare/v6.4.0-next.5...v6.4.0-next.6) (2021-03-19)


### Bug Fixes

* Improves jobs logging ([ebd071e](https://github.com/Elders/Cronus/commit/ebd071e1a7aee86860b4d5b7aa1ff0cb5a40e32b))

# [6.4.0-next.5](https://github.com/Elders/Cronus/compare/v6.4.0-next.4...v6.4.0-next.5) (2021-03-17)


### Bug Fixes

* Code cleanup ([142bf41](https://github.com/Elders/Cronus/commit/142bf417bbf7d55ca0ee0f4b58a0c02049bcf33f))

# [6.4.0-next.4](https://github.com/Elders/Cronus/compare/v6.4.0-next.3...v6.4.0-next.4) (2021-03-17)


### Bug Fixes

* Removes unused contract. sorry ([e1e9ba7](https://github.com/Elders/Cronus/commit/e1e9ba7def331912ed9c9b5937300ae961580186))

# [6.4.0-next.3](https://github.com/Elders/Cronus/compare/v6.4.0-next.2...v6.4.0-next.3) (2021-03-17)


### Bug Fixes

* Adds an async version of the HasSnapshotMarkerAsync ([ff41017](https://github.com/Elders/Cronus/commit/ff410173e85ba0c171e315ef938be56f98c550b5))

# [6.4.0-next.2](https://github.com/Elders/Cronus/compare/v6.4.0-next.1...v6.4.0-next.2) (2021-03-16)


### Bug Fixes

* Fixes projection snapshot revisions ([5d5e74a](https://github.com/Elders/Cronus/commit/5d5e74a5a05cfdd289a97355046358fc39b85c82))

# [6.4.0-next.1](https://github.com/Elders/Cronus/compare/v6.3.0...v6.4.0-next.1) (2021-03-16)


### Bug Fixes

* Reuses the projection index code ([65175da](https://github.com/Elders/Cronus/commit/65175da0daead61160ff3cc392e796a7d370b7da))


### Features

* removes snapshots ([5648518](https://github.com/Elders/Cronus/commit/564851800d2959ac71beb225946b75b6ad240059))

# [6.3.0-next.1](https://github.com/Elders/Cronus/compare/v6.2.10-next.1...v6.3.0-next.1) (2021-02-08)


### Bug Fixes

* Removes gitversion.yml ([388fb24](https://github.com/Elders/Cronus/commit/388fb247a99c9653e0b4c33487ba79439f9b0785))
* Switches to azure pipelines and semantic release ([450f312](https://github.com/Elders/Cronus/commit/450f3123f2284ab4bfb728b62cb30ae403e7fbde))


### Features

* Allows to rebuild non event-sourced projections ([35ce215](https://github.com/Elders/Cronus/commit/35ce215f81bb89b39b33f456a09b6a188d0087e9))
* Orders the projection events by timestamp ([1a57249](https://github.com/Elders/Cronus/commit/1a572490804da9b234010f52524e2ed27fd32901))
* Transfers the release notes to CHANGELOG ([fefd144](https://github.com/Elders/Cronus/commit/fefd144e7a0db741f389182d9b17b7357a6bc1e7))

## [6.2.10-next.1](https://github.com/Elders/Cronus/compare/v6.2.9...v6.2.10-next.1) (2021-01-25)


### Bug Fixes

* Removes gitversion.yml ([388fb24](https://github.com/Elders/Cronus/commit/388fb247a99c9653e0b4c33487ba79439f9b0785))
* Switches to azure pipelines and semantic release ([450f312](https://github.com/Elders/Cronus/commit/450f3123f2284ab4bfb728b62cb30ae403e7fbde))

#### 6.2.9 - 10.12.2020
* Properly handles Event Sourced projections

#### 6.2.8 - 30.11.2020
* Allows publishers to target specific handlers via the recipient_handlers message header

#### 6.2.7 - 05.11.2020
* Fixes projection rebuild rules

#### 6.2.6 - 05.10.2020
* RebuildIndex_EventToAggregateRootId_Job now respects the cancellation token

#### 6.2.5 - 05.10.2020
* Respect the cancellation token when rebuilding a projection

#### 6.2.4 - 02.10.2020
* Fixes IEventStorePlayer discovery registration

#### 6.2.3 - 02.10.2020
* Fixes InMemoryEventStore dependency issue

#### 6.2.2 - 02.10.2020
* Fixes a discovery load issue

#### 6.2.1 - 02.10.2020
* Fixes a discovery load issue

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
