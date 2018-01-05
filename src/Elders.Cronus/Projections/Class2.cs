using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.Projections.Cassandra.EventSourcing
{
    public interface IProjectionRepository
    {
        IProjectionGetResult<T> Get<T>(IBlobId projectionId) where T : IProjectionDefinition;
        void Save(CronusMessage cronusMessage, Type projectionType);
    }

    public class EventSourcedProjectionsMiddleware : Middleware<HandleContext>
    {
        readonly IProjectionRepository repository;

        public EventSourcedProjectionsMiddleware(IProjectionRepository repository)
        {
            if (ReferenceEquals(null, repository) == true) throw new ArgumentNullException(nameof(repository));

            this.repository = repository;
        }

        protected override void Run(Execution<HandleContext> execution)
        {
            CronusMessage cronusMessage = execution.Context.Message;
            if (cronusMessage.Payload is IEvent)
            {
                Type projectionType = execution.Context.HandlerType;
                var projection = FastActivator.CreateInstance(projectionType) as IProjectionDefinition;

                if (projection != null)
                    repository.Save(cronusMessage, projectionType);
            }
        }
    }

    public interface IProjectionVersionResolver
    {
        IEnumerable<ProjectionVersion> GetVersions(Type projectionType);
        IEnumerable<ProjectionVersion> GetVersions(string projectionName);
    }

    public class ProjectionRepository : IProjectionRepository
    {
        readonly IProjectionStore projectionStore;
        readonly ISnapshotStore snapshotStore;
        readonly ISnapshotStrategy snapshotStrategy;
        readonly IProjectionVersionResolver versionResolver;

        public ProjectionRepository(IProjectionStore projectionStore, ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy, IProjectionVersionResolver versionResolver)
        {
            if (ReferenceEquals(null, projectionStore) == true) throw new ArgumentException(nameof(projectionStore));
            if (ReferenceEquals(null, snapshotStore) == true) throw new ArgumentException(nameof(snapshotStore));
            if (ReferenceEquals(null, snapshotStrategy) == true) throw new ArgumentException(nameof(snapshotStrategy));
            if (ReferenceEquals(null, versionResolver) == true) throw new ArgumentException(nameof(versionResolver));

            this.projectionStore = projectionStore;
            this.snapshotStore = snapshotStore;
            this.snapshotStrategy = snapshotStrategy;
            this.versionResolver = versionResolver;
        }

        public IProjectionGetResult<T> Get<T>(IBlobId projectionId) where T : IProjectionDefinition
        {
            string contractId = typeof(T).GetContractId();

            ISnapshot snapshot = snapshotStore.Load(contractId, projectionId);
            ProjectionStream projectionStream = projectionStore.Load(contractId, projectionId, snapshot);
            if (ReferenceEquals(null, projectionStream) == true) throw new ArgumentException(nameof(projectionStream));
            var queryResult = projectionStream.RestoreFromHistory<T>();

            var shouldCreateSnapshot = snapshotStrategy.ShouldCreateSnapshot(projectionStream.Commits, snapshot.Revision);
            if (shouldCreateSnapshot.ShouldCreateSnapshot)
                snapshotStore.Save(new Snapshot(projectionId, contractId, queryResult.Projection.State, shouldCreateSnapshot.KeepTheNextSnapshotRevisionHere));

            return queryResult;
        }

        public void Save(CronusMessage cronusMessage, Type projectionType)
        {
            var projection = FastActivator.CreateInstance(projectionType) as IProjectionDefinition;
            if (projection != null)
            {
                var projectionIds = projection.GetProjectionIds(cronusMessage.Payload as IEvent);
                string contractId = projectionType.GetContractId();

                foreach (var projectionId in projectionIds)
                {
                    ISnapshot snapshot = snapshotStore.Load(contractId, projectionId);
                    ProjectionStream projectionStream = projectionStore.Load(contractId, projectionId, snapshot);
                    if (ReferenceEquals(null, projectionStream) == true) throw new ArgumentException(nameof(projectionStream));
                    var projectionCommits = projectionStream.Commits;
                    int snapshotMarker = snapshotStrategy.GetSnapshotMarker(projectionCommits, snapshot.Revision);

                    EventOrigin eventOrigin = cronusMessage.GetEventOrigin();
                    DateTime timestamp = DateTime.UtcNow;
                    IEvent @event = cronusMessage.Payload as IEvent;
                    foreach (var version in versionResolver.GetVersions(projectionType))
                    {
                        var commit = new ProjectionCommit(projectionId, version, @event, snapshotMarker, eventOrigin, timestamp);
                        projectionStore.Save(commit);
                    }

                    var we = snapshotStrategy.ShouldCreateSnapshot(projectionCommits, snapshot.Revision);
                    if (we.ShouldCreateSnapshot)
                    {
                        var queryResult = projectionStream.RestoreFromHistory(projectionType);
                        snapshotStore.Save(new Snapshot(projectionId, contractId, queryResult.Projection.State, we.KeepTheNextSnapshotRevisionHere));
                    }
                }
            }
        }
    }

    public interface IProjectionStore
    {
        ProjectionStream Load(string contractId, IBlobId projectionId, ISnapshot snapshot);

        void Save(ProjectionCommit commit);

        IProjectionBuilder GetBuilder(Type projectionType);
    }

    public interface ISnapshotStore
    {
        ISnapshot Load(string projectionContractId, IBlobId id);

        void Save(ISnapshot snapshot);
    }

    public interface ISnapshotStrategy
    {
        int GetSnapshotMarker(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision);
        IAmTheAnswerIfWeNeedToCreateSnapshot ShouldCreateSnapshot(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision);
    }

    public interface ISnapshot
    {
        IBlobId Id { get; }
        int Revision { get; }
        object State { get; }
        string ProjectionContractId { get; }
    }

    public class ProjectionStream
    {
        static ILog log = LogProvider.GetLogger(typeof(ProjectionStream));

        private readonly IBlobId projectionId;
        IList<ProjectionCommit> commits;
        readonly ISnapshot snapshot;

        public ProjectionStream(IBlobId projectionId, IList<ProjectionCommit> commits, ISnapshot snapshot)
        {
            if (ReferenceEquals(null, projectionId) == true) throw new ArgumentException(nameof(projectionId));
            if (ReferenceEquals(null, commits) == true) throw new ArgumentException(nameof(commits));
            if (ReferenceEquals(null, snapshot) == true) throw new ArgumentException(nameof(snapshot));

            this.projectionId = projectionId;
            this.commits = commits;
            this.snapshot = snapshot;
        }

        public IEnumerable<ProjectionCommit> Commits { get { return commits.ToList().AsReadOnly(); } }

        public IProjectionGetResult<IProjectionDefinition> RestoreFromHistory(Type projectionType)
        {
            if (commits.Count <= 0 && ReferenceEquals(null, snapshot.State)) return ProjectionGetResult<IProjectionDefinition>.NoResult;

            IProjectionDefinition projection = (IProjectionDefinition)FastActivator.CreateInstance(projectionType, true);
            return RestoreFromHistoryMamamia(projection);
        }

        public IProjectionGetResult<T> RestoreFromHistory<T>() where T : IProjectionDefinition
        {
            if (commits.Count <= 0 && ReferenceEquals(null, snapshot.State)) return ProjectionGetResult<T>.NoResult;

            T projection = (T)Activator.CreateInstance(typeof(T), true);
            return RestoreFromHistoryMamamia<T>(projection);
        }

        IProjectionGetResult<T> RestoreFromHistoryMamamia<T>(T projection) where T : IProjectionDefinition
        {
            projection.InitializeState(projectionId, snapshot.State);

            log.Debug(() => $"Restoring projection `{typeof(T).Name}` from history...{Environment.NewLine}" +
                $"ProjectionId: {Encoding.UTF8.GetString(projection.Id.RawId)}||{Convert.ToBase64String(projection.Id.RawId)}{Environment.NewLine}" +
                $"Snapshot revision: {snapshot.Revision}{Environment.NewLine}" +
                $"MIN/MAX snapshot marker: {commits.Select(x => x.SnapshotMarker).DefaultIfEmpty(snapshot.Revision).Min()}/{commits.Select(x => x.SnapshotMarker).DefaultIfEmpty(snapshot.Revision).Max()}{Environment.NewLine}" +
                $"Projection commits after snapshot: {commits.Count}");

            var groupedBySnapshotMarker = commits.GroupBy(x => x.SnapshotMarker).OrderBy(x => x.Key);
            foreach (var snapshotGroup in groupedBySnapshotMarker)
            {
                var eventsByAggregate = snapshotGroup.GroupBy(x => x.EventOrigin.AggregateRootId);

                foreach (var aggregateGroup in eventsByAggregate)
                {
                    var events = aggregateGroup
                        .OrderBy(x => x.EventOrigin.AggregateRevision)
                        .ThenBy(x => x.EventOrigin.AggregateEventPosition)
                        .Select(x => x.Event);

                    projection.ReplayEvents(events);
                }
            }

            return new ProjectionGetResult<T>(projection);
        }
    }

    public class Snapshot : ISnapshot
    {
        public Snapshot(IBlobId id, string projectionContractId, object state, int revision)
        {
            Id = id;
            ProjectionContractId = projectionContractId;
            State = state;
            Revision = revision;
        }

        public IBlobId Id { get; private set; }

        public string ProjectionContractId { get; private set; }

        public object State { get; set; }

        public int Revision { get; private set; }

        public void InitializeState(object state)
        {
            State = state;
        }
    }

    public class NoSnapshot : ISnapshot
    {
        public NoSnapshot(IBlobId id, string projectionContractId)
        {
            Id = id;
            ProjectionContractId = projectionContractId;
        }

        public IBlobId Id { get; set; }

        public string ProjectionContractId { get; set; }

        public int Revision { get { return 0; } }

        public object State { get; private set; }

        public void InitializeState(object state)
        {

        }
    }

    public interface IProjectionBuilder
    {
        void Begin();

        void Populate(ProjectionCommit commit);

        void End();
    }

    public class DefaultSnapshotStrategy : ISnapshotStrategy
    {
        private TimeSpan snapshotOffset;
        private int eventsInSnapshot;

        public DefaultSnapshotStrategy(TimeSpan snapshotOffset, int eventsInSnapshot)
        {
            this.snapshotOffset = snapshotOffset;
            this.eventsInSnapshot = eventsInSnapshot;
        }

        public int GetSnapshotMarker(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision)
        {
            var lastMarker = commits.Select(x => x.SnapshotMarker).DefaultIfEmpty(lastSnapshotRevision + 1).Max();

            var commitsWithMarker = commits.Where(x => x.SnapshotMarker == lastMarker);

            return commitsWithMarker.Count() >= eventsInSnapshot
                 ? lastMarker + 1
                 : lastMarker;
        }

        public IAmTheAnswerIfWeNeedToCreateSnapshot ShouldCreateSnapshot(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision)
        {
            var commitsAfterLastSnapshotRevision = commits.Where(x => x.SnapshotMarker > lastSnapshotRevision);
            int latestSnapshotMarker = commitsAfterLastSnapshotRevision.Select(x => x.SnapshotMarker).DefaultIfEmpty(lastSnapshotRevision + 1).Max();
            if (latestSnapshotMarker > lastSnapshotRevision)
            {
                bool shouldCreateSnapshot = commitsAfterLastSnapshotRevision.Count() >= eventsInSnapshot || commits.Select(x => x.TimeStamp).DefaultIfEmpty(DateTime.MaxValue).Min() <= DateTime.UtcNow - snapshotOffset;
                if (shouldCreateSnapshot)
                    return new IAmTheAnswerIfWeNeedToCreateSnapshot(latestSnapshotMarker);
            }

            return IAmTheAnswerIfWeNeedToCreateSnapshot.AndInThisCaseTheAnswerIsNo;
        }
    }

    public class IAmTheAnswerIfWeNeedToCreateSnapshot
    {
        public IAmTheAnswerIfWeNeedToCreateSnapshot(int revision)
        {
            KeepTheNextSnapshotRevisionHere = revision;
        }

        public bool ShouldCreateSnapshot { get { return KeepTheNextSnapshotRevisionHere > 0; } }

        public int KeepTheNextSnapshotRevisionHere { get; private set; }

        public static IAmTheAnswerIfWeNeedToCreateSnapshot AndInThisCaseTheAnswerIsNo = new IAmTheAnswerIfWeNeedToCreateSnapshot(-1);
    }

    public class NoSnapshotStore : ISnapshotStore
    {
        public ISnapshot Load(string projectionContractId, IBlobId id)
        {
            return new NoSnapshot(id, projectionContractId);
        }

        public void Save(ISnapshot snapshot)
        {

        }
    }
}
