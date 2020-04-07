using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            isFallbackEnabled = configuration.GetValue<bool>("cronus:projections:fallbackenabled", false) && fallback is null == false;
            useOnlyFallback = configuration.GetValue<bool>("cronus:projections:useonlyfallbackenabled", false) && isFallbackEnabled;
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
            var reporter = new FallbackReporter(this, projectionType);

            try
            {
                primary.Save(projectionType, cronusMessage);
                reporter.PrimaryWriteOK();
            }
            catch (Exception ex)
            {
                reporter.PrimaryWriteFailed(ex, cronusMessage.Payload);
            }

            if (isFallbackEnabled)
            {
                try
                {
                    fallback.Save(projectionType, cronusMessage);
                    reporter.FallbackWriteOK();
                }
                catch (Exception ex)
                {
                    reporter.FallbackWriteFailed(ex, cronusMessage.Payload);
                }
            }

            reporter.ReportAndThrowIfError();
        }

        public void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin)
        {
            var reporter = new FallbackReporter(this, projectionType);

            try
            {
                primary.Save(projectionType, @event, eventOrigin);
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
                    fallback.Save(projectionType, @event, eventOrigin);
                    reporter.FallbackWriteOK();
                }
                catch (Exception ex)
                {
                    reporter.FallbackWriteFailed(ex, @event);
                }
            }

            reporter.ReportAndThrowIfError();
        }

        public void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin, ProjectionVersion version)
        {
            primary.Save(projectionType, @event, eventOrigin, version);
            // this method is specific for the ProjectionsPlayer and it does not make sense to execute the fallback. We need to rethink this
        }

        private ReadResult<T> ExecuteWithFallback<T>(Func<IProjectionReader, ReadResult<T>> func)
        {
            var report = new FallbackReporter(this, typeof(T));

            if (useOnlyFallback && IsFallbackNested == false)
            {
                var result = func(fallback);
                report.FallbackReadResult<T>(result);
                report.Report();

                return result;
            }
            else
            {
                var result = func(primary);
                report.PrimaryReadResult<T>(result);

                if (result.HasError && isFallbackEnabled)
                {
                    result = func(fallback);
                    report.FallbackReadResult<T>(result);
                }
                report.Report();

                return result;
            }
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
            private readonly StringBuilder report;
            private readonly ProjectionRepositoryWithFallback<TPrimary, TFallback> repository;

            List<Exception> exceptions = new List<Exception>();

            public FallbackReporter(ProjectionRepositoryWithFallback<TPrimary, TFallback> repository, Type projectionType)
            {
                this.repository = repository;

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
                    log.WarnException(report.ToString(), exception);
                }
                else
                {
                    log.Info(() => report.ToString());
                }
            }

            public void ReportAndThrowIfError()
            {
                if (exceptions.Any())
                {
                    var exception = new AggregateException(exceptions);
                    log.WarnException(report.ToString(), exception);
                    throw exception;
                }
                else
                {
                    log.Info(() => report.ToString());
                }
            }
        }
    }
}
