using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Cassandra;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections;

/// <summary>
/// Use this class ONLY for version v11 for cronus. Remove this later when we dont use the legacy tables. USE <see cref="ProjectionFinderViaReflection"/> instead
/// We put this because we need to ensure that we have the _new table with the latest live revision (we always append in both projection tables)
/// Otherwise if we have latest live version 6 , until we go manually to rebuild the projection the only _new table for this projection will be the initial `1`
/// </summary>
internal class LatestVersionProjectionFinder : IProjectionVersionFinder
{
    private readonly TypeContainer<IProjection> _allProjections;
    private readonly ProjectionHasher _hasher;
    private readonly IProjectionStore _projectionStore;
    private readonly ICronusContextAccessor _cronusContextAccessor;
    private readonly ILogger<LatestVersionProjectionFinder> _logger;

    private const char Dash = '-';

    private readonly ProjectionVersion persistentVersion;

    public LatestVersionProjectionFinder(TypeContainer<IProjection> allProjections, ProjectionHasher hasher, IProjectionStore projectionStore, ICronusContextAccessor cronusContextAccessor, ILogger<LatestVersionProjectionFinder> logger)
    {
        _allProjections = allProjections;
        _hasher = hasher;
        _projectionStore = projectionStore;
        _cronusContextAccessor = cronusContextAccessor;

        persistentVersion = new ProjectionVersion(ProjectionVersionsHandler.ContractId, ProjectionStatus.Live, 1, hasher.CalculateHash(typeof(ProjectionVersionsHandler)));
        _logger = logger;
    }

    public IEnumerable<ProjectionVersion> GetProjectionVersionsToBootstrap()
    {
        foreach (Type projectionType in _allProjections.Items)
        {
            if (typeof(IProjectionDefinition).IsAssignableFrom(projectionType) || typeof(IAmEventSourcedProjection).IsAssignableFrom(projectionType))
            {
                yield return GetCurrentLiveVersionOrTheDefaultOne(projectionType).GetAwaiter().GetResult();
            }
        }
    }

    public async Task<ProjectionVersion> GetCurrentLiveVersionOrTheDefaultOne(Type projectionType)
    {
        string projectionName = projectionType.GetContractId();

        ProjectionVersionManagerId versionId = new ProjectionVersionManagerId(projectionName, _cronusContextAccessor.CronusContext.Tenant);
        ProjectionVersion initialVersion = new ProjectionVersion(projectionName, ProjectionStatus.NotPresent, 1, _hasher.CalculateHash(projectionType));

        string error = $"table {GetPersistantProjectionColumnFamilyName()} does not exist";

        try
        {
            var loadResultFromF1 = await GetProjectionVersionsFromStoreAsync(projectionName, _cronusContextAccessor.CronusContext.Tenant).ConfigureAwait(false);

            if (loadResultFromF1.IsSuccess)
            {
                ProjectionVersion found = loadResultFromF1.Data.State.AllVersions.GetLive();
                if (found is not null)
                {
                    return found;
                }
                else
                {
                    return initialVersion;
                }
            }
            else
            {
                return initialVersion;
            }
        }
        catch (Exception ex) when (ex.Message.Contains(error)) // we might come here if we are in the initial state (we have no data and f1 projection is missing), because we are trying to load from tables that don't exist yet
        {
            return initialVersion;
        }
        catch (Exception ex) when (True(() => _logger.LogError(ex, "Something went wrong while getting the latest live version, because {message}", ex.Message)))
        {
            throw;
        }
    }

    private async Task<ReadResult<ProjectionVersionsHandler>> GetProjectionVersionsFromStoreAsync(string projectionName, string tenant)
    {
        ProjectionVersionManagerId versionId = new ProjectionVersionManagerId(projectionName, tenant);
        ProjectionStream stream = await LoadProjectionStreamAsync(versionId, persistentVersion).ConfigureAwait(false);

        ProjectionVersionsHandler projectionInstance = new ProjectionVersionsHandler();
        projectionInstance = await stream.RestoreFromHistoryAsync(projectionInstance).ConfigureAwait(false);

        return new ReadResult<ProjectionVersionsHandler>(projectionInstance);
    }

    private async Task<ProjectionStream> LoadProjectionStreamAsync(IBlobId projectionId, ProjectionVersion version)
    {
        List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();

        ProjectionStream stream = ProjectionStream.Empty();

        ProjectionQueryOptions options = new ProjectionQueryOptions(projectionId, version, new PagingOptions(1000, null, Order.Ascending));
        ProjectionsOperator @operator = new ProjectionsOperator()
        {
            OnProjectionStreamLoadedAsync = projectionStream =>
            {
                stream = projectionStream;
                return Task.CompletedTask;
            }
        };

        await _projectionStore.EnumerateProjectionsAsync(@operator, options).ConfigureAwait(false);

        return stream;
    }

    private string GetPersistantProjectionColumnFamilyName()
    {
        string projectionName = persistentVersion.ProjectionName;
        Span<char> result = stackalloc char[projectionName.Length];

        int theIndex = 0;
        for (int i = 0; i < projectionName.Length; i++)
        {
            char character = projectionName[i];

            if (character.Equals(Dash))
                continue;

            if (char.IsUpper(character))
            {
                result[theIndex] = char.ToLower(character);
            }
            else
            {
                result[theIndex] = character;
            }
            theIndex++;
        }
        Span<char> trimmed = result.Slice(0, theIndex);
        return $"{trimmed}_{persistentVersion.Revision}_{persistentVersion.Hash}";
    }

}
