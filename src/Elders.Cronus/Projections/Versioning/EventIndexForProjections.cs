using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = EventTypeIndexForProjections.ContractId)]
    public class EventTypeIndexForProjections : ISubscriber
    {
        public const string ContractId = "13423df4-b815-421d-a53d-c767b157cc81";
        public const string StateId = "55f9e248-7bb3-4288-8db8-ba9620c67228";
        private readonly HashSet<Type> allEventTypesInTheSystem;
        private readonly IProjectionStore store;
        private readonly ICornusInternalStore internalStore;
        private string currentHash;

        public string Id { get { return nameof(EventTypeIndexForProjections); } }

        public EventTypeIndexForProjections(IEnumerable<Type> allEventTypesInTheSystem, IPublisher<ICommand> publisher, IProjectionStore store, ICornusInternalStore internalStore)
        {
            var hasher = new ProjectionHasher();
            //currentHash = hasher.CalculateHash(allEventTypesInTheSystem.Select(x => x.GetContractId()).ToList());
            currentHash = hasher.CalculateHash(typeof(EventTypeIndexForProjections));

            var gg = new ProjectionVersion(ContractId, ProjectionStatus.Live, 1, currentHash);
            store.InitializeProjectionStore(gg);

            this.allEventTypesInTheSystem = new HashSet<Type>(allEventTypesInTheSystem);
            this.store = store;
            this.internalStore = internalStore;
        }

        public IEnumerable<Type> GetInvolvedMessageTypes()
        {
            return allEventTypesInTheSystem;
        }

        private ProjectionVersion Version { get { return new ProjectionVersion(ContractId, ProjectionStatus.Live, 1, currentHash); } }

        public void Process(CronusMessage message)
        {
            var indexId = new EventStoreIndexEventTypeId(message.Payload.GetType().GetContractId());
            var commit = new ProjectionCommit(indexId, Version, (IEvent)message.Payload, 1, message.GetEventOrigin(), DateTime.FromFileTimeUtc(message.GetRootEventTimestamp()));
            store.Save(commit);
        }

        public IEnumerable<ProjectionCommit> EnumerateCommitsByEventType(EventStoreIndexEventTypeId indexId)
        {
            return store.EnumerateProjection(Version, indexId);
        }

        public IndexState GetIndexState()
        {
            try
            {
                var state = internalStore.Load<IndexState>(StateId, currentHash);
                if (state == null)
                    return IndexState.NotPresent;
                else
                    return state;
            }
            catch (Exception)
            {
                return IndexState.NotPresent;
            }
        }
        public IndexBuilder GetIndexBuilder()
        {
            return new IndexBuilder(Version, store, internalStore, currentHash);
        }

        public class IndexBuilder
        {
            private readonly ProjectionVersion indexVersion;
            private readonly IProjectionStore store;
            private readonly ICornusInternalStore internalStore;
            private readonly string indexHash;

            public IndexBuilder(ProjectionVersion indexVersion, IProjectionStore store, ICornusInternalStore internalStore, string indexHash)
            {
                this.indexVersion = indexVersion;
                this.store = store;
                this.internalStore = internalStore;
                this.indexHash = indexHash;
            }

            public void Prepare()
            {
                internalStore.Save(StateId, IndexState.Building, indexHash);
            }

            public void Feed(IEvent @event, EventOrigin origin)
            {
                var indexId = new EventStoreIndexEventTypeId(@event.GetType().GetContractId());
                var commit = new ProjectionCommit(indexId, indexVersion, @event, 1, origin, DateTime.FromFileTimeUtc(origin.Timestamp));
                store.Save(commit);
            }

            public void Complete()
            {
                internalStore.Save(StateId, IndexState.Present, indexHash);
            }
        }
    }

    [DataContract(Name = "fda4a09e-3bd6-46c2-b104-514e6b7166f8")]
    public class IndexState
    {
        public IndexState() { }

        private IndexState(string status)
        {
            this.status = status;
        }

        public bool IsPresent()
        {
            return this.status == Present.status;
        }

        public bool IsBuilding()
        {
            return this.status == Building.status;
        }

        [DataMember(Order = 1)]
        string status;

        public static IndexState NotPresent = new IndexState("notpresent");

        public static IndexState Building = new IndexState("building");

        public static IndexState Present = new IndexState("present");
    }


    public interface ICornusInternalStore
    {
        void Save<T>(string id, T state, string hash);
        T Load<T>(string id, string hash);
    }

    public class StupidProjectionStore : ICornusInternalStore
    {
        private readonly IProjectionStore store;

        public StupidProjectionStore(IProjectionStore store)
        {
            this.store = store;
        }
        public T Load<T>(string id, string hash)
        {
            var version = new ProjectionVersion(typeof(T).GetContractId(), ProjectionStatus.Live, 1, hash);
            var states = store.Load(version, new StupidId(id), 1);
            var lastState = states.OrderByDescending(x => x.TimeStamp).FirstOrDefault();
            return (T)((states.OrderByDescending(x => x.TimeStamp).FirstOrDefault()?.Event as StupidEvent)?.Payload);
        }

        public void Save<T>(string id, T state, string hash)
        {
            var origin = new EventOrigin("StupidProjectionStore", 1, 1, 1);
            var version = new ProjectionVersion(state.GetType().GetContractId(), ProjectionStatus.Live, 1, hash);
            var commit = new ProjectionCommit(new StupidId(id), version, new StupidEvent(state), 1, origin, DateTime.UtcNow);
            store.Save(commit);
        }


        [DataContract(Name = "e8386230-fedd-479e-b5eb-ca0a915deeaa")]
        public class StupidId : IBlobId
        {
            [DataMember(Order = 1)]
            private string stupidString;

            StupidId() { }

            public StupidId(string contractId)
            {
                if (string.IsNullOrEmpty(contractId)) throw new ArgumentNullException(nameof(contractId));

                this.stupidString = contractId;
            }

            public StupidId(Type eventType) : this(eventType.GetContractId()) { }

            public byte[] RawId { get { return Encoding.UTF8.GetBytes(stupidString); } }
        }
        [DataContract(Name = "06c0af68-5506-4b89-a20d-ac1100531e29")]
        public class StupidEvent : IEvent
        {
            public StupidEvent(object payload)
            {
                Payload = payload;
            }

            [DataMember(Order = 1)]
            public object Payload { get; private set; }
        }
    }
}
