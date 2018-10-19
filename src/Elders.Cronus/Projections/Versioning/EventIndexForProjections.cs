using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = IndexEventTypeSubscriber.ContractId)]
    public class IndexEventTypeSubscriber : ISubscriber
    {
        public const string ContractId = "13423df4-b815-421d-a53d-c767b157cc81";

        private readonly TypeContainer<IEvent> allEventTypesInTheSystem;
        private readonly IProjectionStore projectionStore;
        private string currentHash;
        private readonly ProjectionVersion projectionVersion;

        public string Id { get { return nameof(IndexEventTypeSubscriber); } }

        public IndexEventTypeSubscriber(TypeContainer<IEvent> allEventTypesInTheSystem, IPublisher<ICommand> publisher, IProjectionStore projectionStore)
        {
            var hasher = new ProjectionHasher();
            this.currentHash = hasher.CalculateHash(typeof(IndexEventTypeSubscriber));

            this.projectionVersion = new ProjectionVersion(ContractId, ProjectionStatus.Live, 1, currentHash);
            this.allEventTypesInTheSystem = allEventTypesInTheSystem;
            //projectionStore.InitializeProjectionStores(version);


            this.projectionStore = projectionStore;
        }

        public IEnumerable<Type> GetInvolvedMessageTypes()
        {
            return allEventTypesInTheSystem.Items;
        }

        public void Process(CronusMessage message)
        {
            var indexId = new EventStoreIndexEventTypeId(message.Payload.GetType().GetContractId());
            var commit = new ProjectionCommit(indexId, projectionVersion, (IEvent)message.Payload, 1, message.GetEventOrigin(), DateTime.FromFileTimeUtc(message.GetRootEventTimestamp()));
            projectionStore.Save(commit);
        }

        //public IEnumerable<ProjectionCommit> EnumerateCommitsByEventType(EventStoreIndexEventTypeId indexId)
        //{
        //    return factory.GetProjectionStore(CronusAssembly.EldersTenant).EnumerateProjection(Version, indexId);
        //}


        //public IndexBuilder GetIndexBuilder()
        //{
        //    return new IndexBuilder(Version, factory, internalStore);
        //}

    }
}
