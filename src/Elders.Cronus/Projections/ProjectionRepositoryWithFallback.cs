using System;
using System.Threading.Tasks;
using Elders.Cronus.Logging;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.Projections
{
    public interface IAmCanExecuteFallback
    {
        bool IsFallbackNested { get; set; }
    }

    public class ProjectionRepositoryWithFallback<TFallback> : ProjectionRepositoryWithFallback<ProjectionRepository, TFallback>
           where TFallback : class, IProjectionReader, IProjectionWriter
    {
        public ProjectionRepositoryWithFallback(IConfiguration configuration, ProjectionRepository main, TFallback fallback)
            : base(configuration, main, fallback) { }
    }

    public class ProjectionRepositoryWithFallback<TPrimary, TFallback> : IProjectionReader, IProjectionWriter, IAmCanExecuteFallback
        where TPrimary : class, IProjectionReader, IProjectionWriter
        where TFallback : class, IProjectionReader, IProjectionWriter
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(ProjectionRepositoryWithFallback<,>));

        private readonly bool isFallbackEnabled;
        private readonly bool useOnlyFallback;
        private readonly TPrimary primary;
        private readonly TFallback fallback;

        public bool IsFallbackNested { get; set; }

        public ProjectionRepositoryWithFallback(IConfiguration configuration, TPrimary primary, TFallback fallback)
        {
            isFallbackEnabled = configuration.GetValue<bool>("cronus_projections_fallback_enabled", false) && fallback is null == false;
            useOnlyFallback = configuration.GetValue<bool>("cronus_projections_useonlyfallback_enabled", false) && isFallbackEnabled;
            this.primary = primary;
            this.fallback = fallback;

            CheckAndMarkIfFallbackIsNested(fallback);
        }

        void CheckAndMarkIfFallbackIsNested(TFallback fallback)
        {
            if (typeof(IAmCanExecuteFallback).IsAssignableFrom(fallback.GetType()))
            {
                (fallback as IAmCanExecuteFallback).IsFallbackNested = true;
            }
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
            primary.Save(projectionType, cronusMessage);
            if (isFallbackEnabled)
                fallback.Save(projectionType, cronusMessage);
        }

        public void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin)
        {
            primary.Save(projectionType, @event, eventOrigin);
            if (isFallbackEnabled)
                fallback.Save(projectionType, @event, eventOrigin);
        }

        public void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin, ProjectionVersion version)
        {
            primary.Save(projectionType, @event, eventOrigin, version);
            // this method is specific for the ProjectionsPlayer and it does not make sense to execute the fallback. We need to rethink this
        }

        private ReadResult<T> ExecuteWithFallback<T>(Func<IProjectionReader, ReadResult<T>> func)
        {
            if (useOnlyFallback && IsFallbackNested == false)
            {
                return func(fallback);
            }
            else
            {
                var result = func(primary);
                if (result.HasError && isFallbackEnabled)
                {
                    log.Info(() => $"Primary projection has failed. Falling back... Primary: {primary.ToString()} Fallback: {fallback.ToString()}");
                    result = func(fallback);
                }

                return result;
            }
        }

        private async Task<ReadResult<T>> ExecuteWithFallbackAsync<T>(Func<IProjectionReader, Task<ReadResult<T>>> func)
        {
            if (useOnlyFallback && IsFallbackNested == false)
            {
                return await func(fallback).ConfigureAwait(false);
            }
            else
            {
                var result = await func(primary).ConfigureAwait(false);
                if (result.HasError && isFallbackEnabled)
                {
                    log.Info(() => $"Primary projection has failed. Falling back... Primary: {primary.ToString()} Fallback: {fallback.ToString()}");
                    result = await func(fallback).ConfigureAwait(false);
                }

                return result;
            }
        }
    }
}
