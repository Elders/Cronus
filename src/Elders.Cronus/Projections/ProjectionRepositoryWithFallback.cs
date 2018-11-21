using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.Projections
{
    public class ProjectionRepositoryWithFallback<TFallback> : ProjectionRepositoryWithFallback<ProjectionRepository, TFallback>
           where TFallback : class, IProjectionReader, IProjectionWriter
    {
        public ProjectionRepositoryWithFallback(IConfiguration configuration, ProjectionRepository main, TFallback fallback)
            : base(configuration, main, fallback) { }
    }

    public class ProjectionRepositoryWithFallback<TMain, TFallback> : IProjectionReader, IProjectionWriter
        where TMain : class, IProjectionReader, IProjectionWriter
        where TFallback : class, IProjectionReader, IProjectionWriter
    {
        private readonly bool isFallbackEnabled;
        private readonly bool useOnlyFallback;
        private readonly TMain main;
        private readonly TFallback fallback;

        public ProjectionRepositoryWithFallback(IConfiguration configuration, TMain main, TFallback fallback)
        {
            isFallbackEnabled = configuration.GetValue<bool>("cronus_projections_fallback_enabled", false) && fallback is null == false;
            useOnlyFallback = configuration.GetValue<bool>("cronus_projections_useonlyfallback_enabled", false) && isFallbackEnabled;
            this.main = main;
            this.fallback = fallback;
        }

        public ReadResult<T> Get<T>(IBlobId projectionId) where T : IProjectionDefinition
        {
            return ExecuteWithFallback(repo => repo.Get<T>(projectionId));
        }

        public ReadResult<IProjectionDefinition> Get(IBlobId projectionId, Type projectionType)
        {
            return ExecuteWithFallback(repo => repo.Get(projectionId, projectionType));
        }

        public Task<ReadResult<T>> GetAsync<T>(IBlobId projectionId) where T : IProjectionDefinition
        {
            return ExecuteWithFallbackAsync(repo => repo.GetAsync<T>(projectionId));
        }

        public Task<ReadResult<IProjectionDefinition>> GetAsync(IBlobId projectionId, Type projectionType)
        {
            return ExecuteWithFallbackAsync(repo => repo.GetAsync(projectionId, projectionType));
        }

        public void Save(Type projectionType, CronusMessage cronusMessage)
        {
            main.Save(projectionType, cronusMessage);
            if (isFallbackEnabled)
                fallback.Save(projectionType, cronusMessage);
        }

        public void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin)
        {
            main.Save(projectionType, @event, eventOrigin);
            if (isFallbackEnabled)
                fallback.Save(projectionType, @event, eventOrigin);
        }

        public void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin, ProjectionVersion version)
        {
            main.Save(projectionType, @event, eventOrigin);
            // this method is specific for the ProjectionsPlayer and it does not make sense to execute the fallback. We need to rethink this
        }

        private ReadResult<T> ExecuteWithFallback<T>(Func<IProjectionReader, ReadResult<T>> func)
        {
            if (useOnlyFallback)
            {
                return func(fallback);
            }
            else
            {
                var result = func(main);
                if (result.HasFailed && isFallbackEnabled)
                    result = func(fallback);

                return result;
            }
        }

        private async Task<ReadResult<T>> ExecuteWithFallbackAsync<T>(Func<IProjectionReader, Task<ReadResult<T>>> func)
        {
            if (useOnlyFallback)
            {
                return await func(fallback).ConfigureAwait(false);
            }
            else
            {
                var result = await func(main).ConfigureAwait(false);
                if (result.HasFailed && isFallbackEnabled)
                    result = await func(fallback).ConfigureAwait(false);

                return result;
            }
        }
    }
}
