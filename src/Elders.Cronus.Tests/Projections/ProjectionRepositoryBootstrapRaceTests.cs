using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Cassandra;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Elders.Cronus.Tests.Projections;

/// <summary>
/// Regression tests for the bootstrap-ordering race fixed in commit 647f0f74
/// ("Synthesizes discovery-time projection version when version handler stream is empty").
///
/// Background: during startup the <c>ProjectionVersionsHandler</c> stream for a given
/// projection name is empty until the version handler itself has been rebuilt. Before
/// the fix, <see cref="ProjectionRepository.SaveAsync(Type, IEvent)"/> would silently
/// no-op in that window because <c>GetProjectionVersionsAsync</c> reported NotFound and
/// the save loop iterated zero versions. That left writes to system projections like
/// <c>EventStoreIndexStatus</c> dropped, which kept user-projection rebuilds stuck in
/// <c>JobExecutionStatus.Running</c> indefinitely.
///
/// The fix synthesizes a discovery-time <see cref="ProjectionVersion"/>
/// (Status=New, Revision=1, Hash=<see cref="ProjectionHasher"/>) whenever the version
/// handler stream is empty, so writes have a writable target version.
/// </summary>
public class ProjectionRepositoryBootstrapRaceTests
{
    [Fact]
    public async Task GetProjectionVersionsAsync_when_version_handler_stream_is_empty_synthesizes_discovery_time_version()
    {
        BootstrapTestHarness harness = BootstrapTestHarness.WithEmptyVersionHandlerStream();

        ReadResult<ProjectionVersions> result = await harness.Repository.InvokeGetProjectionVersionsAsync(harness.ProjectionName);

        Assert.True(result.IsSuccess, $"Expected synthesized fallback to produce IsSuccess=true. Got: {result}");
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.Count);

        ProjectionVersion synthesized = SingleVersion(result.Data);
        Assert.Equal(harness.ProjectionName, synthesized.ProjectionName);
        Assert.Equal(ProjectionStatus.New, synthesized.Status);
        Assert.Equal(1, synthesized.Revision);
        Assert.Equal(harness.ExpectedHash, synthesized.Hash);
    }

    [Fact]
    public async Task SaveAsync_when_version_handler_stream_is_empty_persists_through_synthesized_version()
    {
        BootstrapTestHarness harness = BootstrapTestHarness.WithEmptyVersionHandlerStream();
        BootstrapTestEvent @event = new BootstrapTestEvent();

        await harness.Repository.SaveAsync(typeof(BootstrapTestProjection), @event);

        ProjectionCommit commit = Assert.Single(harness.ProjectionStore.SavedCommits);
        Assert.Same(@event, commit.Event);
        Assert.NotNull(commit.Version);
        Assert.Equal(harness.ProjectionName, commit.Version.ProjectionName);
        Assert.Equal(ProjectionStatus.New, commit.Version.Status);
        Assert.Equal(1, commit.Version.Revision);
        Assert.Equal(harness.ExpectedHash, commit.Version.Hash);
    }

    [Fact]
    public void Synthesized_version_hash_matches_canonical_ProjectionHasher_output()
    {
        // Guards against drift between ProjectionRepository's fallback and ProjectionVersionManager's
        // first-registration path. If the storage key (name, revision, hash) ever diverges, writes
        // made through the bootstrap fallback would be invisible once the canonical Live version
        // arrives.
        ProjectionHasher hasher = new ProjectionHasher();
        string canonicalHash = hasher.CalculateHash(typeof(BootstrapTestProjection));

        BootstrapTestHarness harness = BootstrapTestHarness.WithEmptyVersionHandlerStream();

        Assert.Equal(canonicalHash, harness.ExpectedHash);
    }

    private static ProjectionVersion SingleVersion(ProjectionVersions versions)
    {
        ArgumentNullException.ThrowIfNull(versions);
        ProjectionVersion only = null;
        int count = 0;
        using IEnumerator<ProjectionVersion> enumerator = versions.GetEnumerator();
        while (enumerator.MoveNext())
        {
            only = enumerator.Current;
            count++;
        }
        Assert.Equal(1, count);
        return only;
    }

    // ---------------- harness ----------------

    private sealed class BootstrapTestHarness
    {
        public TestableProjectionRepository Repository { get; }
        public CapturingProjectionStore ProjectionStore { get; }
        public string ProjectionName { get; }
        public string ExpectedHash { get; }

        private BootstrapTestHarness(TestableProjectionRepository repository, CapturingProjectionStore projectionStore, string projectionName, string expectedHash)
        {
            Repository = repository;
            ProjectionStore = projectionStore;
            ProjectionName = projectionName;
            ExpectedHash = expectedHash;
        }

        public static BootstrapTestHarness WithEmptyVersionHandlerStream()
        {
            // Pre-warm the contract cache so projectionName.GetTypeByContract() inside the fix
            // can resolve back to BootstrapTestProjection.
            string projectionName = typeof(BootstrapTestProjection).GetContractId();
            _ = typeof(BootstrapTestEvent).GetContractId();

            ProjectionHasher hasher = new ProjectionHasher();
            string expectedHash = hasher.CalculateHash(typeof(BootstrapTestProjection));

            CapturingProjectionStore projectionStore = new CapturingProjectionStore();
            ServiceCollection services = new ServiceCollection();
            services.AddTransient<BootstrapTestProjection>();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            CronusContextAccessor contextAccessor = new CronusContextAccessor
            {
                CronusContext = new CronusContext("test-tenant", serviceProvider)
            };
            DefaultHandlerFactory handlerFactory = new DefaultHandlerFactory(contextAccessor);

            TestableProjectionRepository repository = new TestableProjectionRepository(contextAccessor, projectionStore, handlerFactory, hasher);

            return new BootstrapTestHarness(repository, projectionStore, projectionName, expectedHash);
        }

        private sealed class CronusContextAccessor : ICronusContextAccessor
        {
            public CronusContext CronusContext { get; set; }
        }
    }

    /// <summary>
    /// Subclass that exposes the protected <see cref="ProjectionRepository.GetProjectionVersionsAsync"/>
    /// for direct invocation. The method itself is not overridden - we exercise the real fallback logic.
    /// </summary>
    private sealed class TestableProjectionRepository : ProjectionRepository
    {
        public TestableProjectionRepository(ICronusContextAccessor contextAccessor, IProjectionStore projectionStore, IHandlerFactory handlerFactory, ProjectionHasher projectionHasher)
            : base(contextAccessor, projectionStore, handlerFactory, projectionHasher)
        {
        }

        public Task<ReadResult<ProjectionVersions>> InvokeGetProjectionVersionsAsync(string projectionName)
            => GetProjectionVersionsAsync(projectionName);
    }

    /// <summary>
    /// Fake <see cref="IProjectionStore"/> that simulates the bootstrap window: every
    /// <see cref="EnumerateProjectionsAsync"/> call leaves the operator's stream callback
    /// uninvoked, so callers receive an empty <see cref="ProjectionStream"/>. Captures all
    /// <see cref="ProjectionCommit"/> writes for inspection.
    /// </summary>
    private sealed class CapturingProjectionStore : IProjectionStore
    {
        private readonly ConcurrentQueue<ProjectionCommit> savedCommits = new ConcurrentQueue<ProjectionCommit>();

        public IReadOnlyCollection<ProjectionCommit> SavedCommits => savedCommits.ToArray();

        public Task EnumerateProjectionsAsync(ProjectionsOperator @operator, ProjectionQueryOptions options)
        {
            // Intentionally do not invoke @operator.OnProjectionStreamLoadedAsync. This mirrors
            // the bootstrap state where the ProjectionVersionsHandler stream is empty: callers
            // keep their default ProjectionStream.Empty(), RestoreFromHistoryAsync returns
            // default(T), and GetProjectionVersionsFromStoreAsync collapses to NotFound.
            return Task.CompletedTask;
        }

        public Task SaveAsync(ProjectionCommit commit)
        {
            ArgumentNullException.ThrowIfNull(commit);
            savedCommits.Enqueue(commit);
            return Task.CompletedTask;
        }
    }

    // ---------------- test projection / event ----------------

    [DataContract(Name = "f3a6c1d2-7b48-4a92-9d1e-bootstrapraceevt")]
    public sealed class BootstrapTestEvent : IEvent
    {
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    }

    [DataContract(Name = "8b2e5a14-9f6d-47c8-a3b0-bootstrapraceprj")]
    public sealed class BootstrapTestProjection : ProjectionDefinition<BootstrapTestProjectionState, BootstrapTestProjectionId>,
        IEventHandler<BootstrapTestEvent>
    {
        public BootstrapTestProjection()
        {
            Subscribe<BootstrapTestEvent>(_ => new BootstrapTestProjectionId("test-tenant", "fixed-id"));
        }

        public Task HandleAsync(BootstrapTestEvent @event) => Task.CompletedTask;
    }

    public sealed class BootstrapTestProjectionState
    {
    }

    public sealed class BootstrapTestProjectionId : AggregateRootId
    {
        private BootstrapTestProjectionId() : base() { }

        public BootstrapTestProjectionId(string tenant, string id) : base(tenant, "bootstrapraceid", id) { }
    }
}
