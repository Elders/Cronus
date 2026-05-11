using System;
using System.Threading.Tasks;
using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Rebuilding;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Elders.Cronus.Tests.Projections.Rebuilding;

public class ProjectionVersionHelperTests
{
    [Fact]
    public async Task InitializeNewProjectionVersionAsync_propagates_initializer_exceptions()
    {
        // Before the await fix, InitializeNewProjectionVersion was void and swallowed every failure
        // from IInitializableProjectionStore.InitializeAsync. Callers had no way to learn the version
        // tracker was not actually initialized and looped on retry. The async variant must surface
        // these failures so RebuildProjection_Job transitions to JobExecutionStatus.Failed instead of
        // staying silently stuck in Running.
        InvalidOperationException expected = new InvalidOperationException("storage adapter exploded");
        ThrowingInitializableProjectionStore initializer = new ThrowingInitializableProjectionStore(expected);

        ProjectionVersionHelper helper = new ProjectionVersionHelper(
            contextAccessor: null,
            projectionReader: null,
            projectionVersionInitializer: initializer,
            projectionHasher: new ProjectionHasher(),
            logger: NullLogger<ProjectionVersionHelper>.Instance);

        InvalidOperationException actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => helper.InitializeNewProjectionVersionAsync());

        Assert.Same(expected, actual);
        Assert.Equal(1, initializer.CallCount);
    }

    private sealed class ThrowingInitializableProjectionStore : IInitializableProjectionStore
    {
        private readonly Exception toThrow;

        public ThrowingInitializableProjectionStore(Exception toThrow)
        {
            this.toThrow = toThrow;
        }

        public int CallCount { get; private set; }

        public Task<bool> InitializeAsync(ProjectionVersion version)
        {
            CallCount++;
            throw toThrow;
        }
    }
}
