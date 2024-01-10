using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections
{
    public interface ICanExecuteFallback
    {
        bool IsFallbackNested { get; set; }
    }

    public class ProjectionRepositoryWithFallback<TFallback> : ProjectionRepositoryWithFallback<ProjectionRepository, TFallback>
           where TFallback : class, IProjectionReader, IProjectionWriter
    {
        public ProjectionRepositoryWithFallback(IConfiguration configuration, ProjectionRepository main, TFallback fallback)
            : base(configuration, main, fallback) { }
    }

    public class ProjectionRepositoryWithFallback<TPrimary, TFallback> : IProjectionReader, IProjectionWriter, ICanExecuteFallback
        where TPrimary : class, IProjectionReader, IProjectionWriter
        where TFallback : class, IProjectionReader, IProjectionWriter
    {
        private readonly bool isFallbackEnabled;
        private readonly bool useOnlyFallback;
        private readonly TPrimary primary;
        private readonly TFallback fallback;

        public bool IsFallbackNested { get; set; }

        public ProjectionRepositoryWithFallback(IConfiguration configuration, TPrimary primary, TFallback fallback)
        {
            isFallbackEnabled = configuration.GetValue<bool>("cronus:projections:fallbackenabled", false) && fallback is null == false;
            useOnlyFallback = configuration.GetValue<bool>("cronus:projections:useonlyfallbackenabled", false) && isFallbackEnabled;
            this.primary = primary;
            this.fallback = fallback;

            CheckAndMarkIfFallbackIsNested(fallback);
        }

        void CheckAndMarkIfFallbackIsNested(TFallback fallback)
        {
            if (typeof(ICanExecuteFallback).IsAssignableFrom(fallback.GetType()))
            {
                (fallback as ICanExecuteFallback).IsFallbackNested = true;
            }
        }

        public Task<ReadResult<T>> GetAsync<T>(IBlobId projectionId) where T : IProjectionDefinition
        {
            return ExecuteWithFallbackAsync(repo => repo.GetAsync<T>(projectionId));
        }

        public Task<ReadResult<IProjectionDefinition>> GetAsync(IBlobId projectionId, Type projectionType)
        {
            return ExecuteWithFallbackAsync(repo => repo.GetAsync(projectionId, projectionType));
        }

        public Task<ReadResult<T>> GetAsOfAsync<T>(IBlobId projectionId, DateTimeOffset timestamp) where T : IProjectionDefinition
        {
            return ExecuteWithFallbackAsync(repo => repo.GetAsOfAsync<T>(projectionId, timestamp));
        }

        public async Task SaveAsync(Type projectionType, IEvent @event)
        {
            var reporter = new FallbackReporter(this, projectionType);

            try
            {
                await primary.SaveAsync(projectionType, @event).ConfigureAwait(false);
                reporter.PrimaryWriteOK();
            }
            catch (Exception ex)
            {
                reporter.PrimaryWriteFailed(ex, @event);
            }
            if (isFallbackEnabled)
            {
                try
                {
                    await fallback.SaveAsync(projectionType, @event).ConfigureAwait(false);
                    reporter.FallbackWriteOK();
                }
                catch (Exception ex)
                {
                    reporter.FallbackWriteFailed(ex, @event);
                }
            }

            reporter.ReportAndThrowIfError();
        }

        public async Task SaveAsync(Type projectionType, IEvent @event, ProjectionVersion version)
        {
            await primary.SaveAsync(projectionType, @event, version).ConfigureAwait(false);
            // this method is specific for the ProjectionsPlayer and it does not make sense to execute the fallback. We need to rethink this
        }

        private async Task<ReadResult<T>> ExecuteWithFallbackAsync<T>(Func<IProjectionReader, Task<ReadResult<T>>> func)
        {
            var reporter = new FallbackReporter(this, typeof(T));

            if (useOnlyFallback && IsFallbackNested == false)
            {
                var result = await func(fallback).ConfigureAwait(false);
                reporter.FallbackReadResult<T>(result);
                reporter.Report();

                return result;
            }
            else
            {
                var result = await func(primary).ConfigureAwait(false);
                reporter.PrimaryReadResult<T>(result);

                if (result.HasError && isFallbackEnabled)
                {
                    result = await func(fallback).ConfigureAwait(false);
                    reporter.FallbackReadResult<T>(result);
                }
                reporter.Report();

                return result;
            }
        }

        public class FallbackReporter
        {
            private readonly ILogger logger;
            private readonly StringBuilder report;
            private readonly ProjectionRepositoryWithFallback<TPrimary, TFallback> repository;

            List<Exception> exceptions = new List<Exception>();

            public FallbackReporter(ProjectionRepositoryWithFallback<TPrimary, TFallback> repository, Type projectionType, ILogger logger = null)
            {
                this.repository = repository;
                this.logger = logger;

                report = ReportPrepare(projectionType);
            }

            StringBuilder ReportPrepare(Type projectionType)
            {
                StringBuilder report = new StringBuilder();
                report.AppendLine($"Projection report using fallback. | ProjectionType: {projectionType.Name}");
                report.AppendLine($"Primary: {repository.primary.GetType().Name}");
                report.AppendLine($"Fallback: {repository.fallback.GetType().Name} | FallbackEnabled: {repository.isFallbackEnabled} | UseOnlyFallback: {repository.useOnlyFallback} | IsFallbackNested: {repository.IsFallbackNested}");

                return report;
            }

            public void PrimaryReadOK() => report.AppendLine("Primary read: OK");
            public void FallbackReadOK() => report.AppendLine("Fallback read: OK");

            public void PrimaryReadResult<T>(ReadResult<T> result)
            {
                if (result.IsSuccess)
                    PrimaryReadOK();

                else if (result.HasError)
                    PrimaryReadFailed($"{result.Error} - {result.NotFoundHint}");
            }

            public void FallbackReadResult<T>(ReadResult<T> result)
            {
                if (result.IsSuccess)
                    FallbackReadOK();

                else if (result.HasError)
                    FallbackReadFailed($"{result.Error} - {result.NotFoundHint}");
            }

            public void PrimaryReadFailed(string error)
            {
                exceptions.Add(new Exception(error));
                report.AppendLine($"Primary read: FAILED - {error}");
            }

            public void FallbackReadFailed(string error)
            {
                exceptions.Add(new Exception(error));
                report.AppendLine($"Fallback read: FAILED - {error}");
            }

            public void PrimaryWriteOK() => report.AppendLine("Primary write: OK");
            public void FallbackWriteOK() => report.AppendLine("Fallback write: OK");

            public void PrimaryWriteFailed(Exception error, IMessage message)
            {
                exceptions.Add(error);
                report.AppendLine($"Primary write: FAILED - {error.Message} | Event: {message.GetType().Name}");
            }

            public void FallbackWriteFailed(Exception error, IMessage message)
            {
                exceptions.Add(error);
                report.AppendLine($"Fallback write: FAILED - {error.Message} | Event: {message.GetType().Name}");
            }

            public void Report()
            {
                if (exceptions.Any())
                {
                    var exception = new AggregateException(exceptions);
                    logger.WarnException(exception, () => report.ToString());
                }
                else
                {
                    logger.Info(() => report.ToString());
                }
            }

            public void ReportAndThrowIfError()
            {
                if (exceptions.Any())
                {
                    var exception = new AggregateException(exceptions);
                    logger.WarnException(exception, () => report.ToString());
                    throw exception;
                }
                else
                {
                    logger.Info(() => report.ToString());
                }
            }
        }
    }
}
