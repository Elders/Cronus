using System;
using System.Collections.Generic;
using Netflix.Hystrix;
using Netflix.Hystrix.CircuitBreaker;
using Netflix.Hystrix.Strategy.Metrics;
using Netflix.Hystrix.Util;
using Netflix.Servo;
using Netflix.Servo.Attributes;
using Netflix.Servo.Monitor;
using Netflix.Servo.Tag;
using Servo.NET.Atlas;

namespace Hystrix.Contrib.ServoPublisher
{
    public class HystrixServoMetricsPublisherCommand : HystrixServoMetricsPublisherAbstract, IHystrixMetricsPublisherCommand
    {
        readonly HystrixCommandKey commandKey;
        readonly HystrixCommandGroupKey commandGroupKey;
        readonly HystrixCommandMetrics metrics;
        readonly IHystrixCircuitBreaker circuitBreaker;
        readonly IHystrixCommandProperties properties;

        readonly ITag servoInstanceTag;
        readonly ITag servoTypeTag;

        public HystrixServoMetricsPublisherCommand(
            HystrixCommandKey commandKey,
            HystrixCommandGroupKey commandGroupKey,
            HystrixCommandMetrics metrics,
            IHystrixCircuitBreaker circuitBreaker,
            IHystrixCommandProperties properties)
        {
            this.commandKey = commandKey;
            this.commandGroupKey = commandGroupKey;
            this.metrics = metrics;
            this.circuitBreaker = circuitBreaker;
            this.properties = properties;

            this.servoInstanceTag = new BasicTag("instance", commandKey.Name);
            this.servoTypeTag = new BasicTag("origin", "HystrixCommand");
        }

        protected override ITag ServoTypeTag { get { return servoTypeTag; } }

        protected override ITag ServoInstanceTag { get { return servoInstanceTag; } }

        public void Initialize()
        {
            List<IMonitor> monitors = getServoMonitors();

            // publish metrics together under a single composite (it seems this name is ignored)
            MonitorConfig commandMetricsConfig = MonitorConfig.builder("HystrixCommand_" + commandKey.Name).build();
            BasicCompositeMonitor commandMetricsMonitor = new BasicCompositeMonitor(commandMetricsConfig, monitors);

            DefaultMonitorRegistry.getInstance().register(commandMetricsMonitor);
        }

        /**
     * Servo will flatten metric names as: getServoTypeTag()_getServoInstanceTag()_monitorName
     */
        private List<IMonitor> getServoMonitors()
        {

            List<IMonitor> monitors = new List<IMonitor>();

            monitors.Add(ServoMetricFactory.InformationalMetric<bool>(this,
                MonitorConfig.builder("isCircuitBreakerOpen").build(),
                () => circuitBreaker.IsOpen()));

            // allow Servo and monitor to know exactly at what point in time these stats are for so they can be plotted accurately
            monitors.Add(getCurrentValueMonitor("currentTime", () => DateTime.UtcNow.ToUnixTimestamp(), DataSourceLevel.DEBUG));

            // cumulative counts
            //monitors.Add(getCumulativeMonitor("countBadRequests", HystrixEventType.BAD_REQUEST));
            //            monitors.add(getCumulativeMonitor("countCollapsedRequests", HystrixEventType.COLLAPSED));
            //            monitors.add(getCumulativeMonitor("countEmit", HystrixEventType.EMIT));
            monitors.Add(getCumulativeMonitor("countExceptionsThrown", HystrixEventType.ExceptionThrown));
            monitors.Add(getCumulativeMonitor("countFailure", HystrixEventType.Failure));
            //            monitors.add(getCumulativeMonitor("countFallbackEmit", HystrixEventType.FALLBACK_EMIT));
            monitors.Add(getCumulativeMonitor("countFallbackFailure", HystrixEventType.FallbackFailure));
            //            monitors.add(getCumulativeMonitor("countFallbackMissing", HystrixEventType.FALLBACK_MISSING));
            monitors.Add(getCumulativeMonitor("countFallbackRejection", HystrixEventType.FallbackRejection));
            monitors.Add(getCumulativeMonitor("countFallbackSuccess", HystrixEventType.FallbackSuccess));
            monitors.Add(getCumulativeMonitor("countResponsesFromCache", HystrixEventType.ResponseFromCache));
            monitors.Add(getCumulativeMonitor("countSemaphoreRejected", HystrixEventType.SemaphoreRejected));
            monitors.Add(getCumulativeMonitor("countShortCircuited", HystrixEventType.ShortCircuited));
            monitors.Add(getCumulativeMonitor("countSuccess", HystrixEventType.Success));
            monitors.Add(getCumulativeMonitor("countThreadPoolRejected", HystrixEventType.ThreadPoolRejected));
            monitors.Add(getCumulativeMonitor("countTimeout", HystrixEventType.Timeout));

            // rolling counts
            //            monitors.add(getRollingMonitor("rollingCountBadRequests", HystrixEventType.BAD_REQUEST));
            //            monitors.add(getRollingMonitor("rollingCountCollapsedRequests", HystrixEventType.COLLAPSED));
            //            monitors.add(getRollingMonitor("rollingCountEmit", HystrixEventType.EMIT));
            monitors.Add(getRollingMonitor("rollingCountExceptionsThrown", HystrixEventType.ExceptionThrown));
            monitors.Add(getRollingMonitor("rollingCountFailure", HystrixEventType.Failure));
            //monitors.Add(getRollingMonitor("rollingCountFallbackEmit", HystrixEventType.FALLBACK_EMIT));
            monitors.Add(getRollingMonitor("rollingCountFallbackFailure", HystrixEventType.FallbackFailure));
            //monitors.Add(getRollingMonitor("rollingCountFallbackMissing", HystrixEventType.FALLBACK_MISSING));
            monitors.Add(getRollingMonitor("rollingCountFallbackRejection", HystrixEventType.FallbackRejection));
            monitors.Add(getRollingMonitor("rollingCountFallbackSuccess", HystrixEventType.FallbackSuccess));
            monitors.Add(getRollingMonitor("rollingCountResponsesFromCache", HystrixEventType.ResponseFromCache));
            monitors.Add(getRollingMonitor("rollingCountSemaphoreRejected", HystrixEventType.SemaphoreRejected));
            monitors.Add(getRollingMonitor("rollingCountShortCircuited", HystrixEventType.ShortCircuited));
            monitors.Add(getRollingMonitor("rollingCountSuccess", HystrixEventType.Success));
            monitors.Add(getRollingMonitor("rollingCountThreadPoolRejected", HystrixEventType.ThreadPoolRejected));
            monitors.Add(getRollingMonitor("rollingCountTimeout", HystrixEventType.Timeout));

            // the number of executionSemaphorePermits in use right now
            monitors.Add(getCurrentValueMonitor("executionSemaphorePermitsInUse", () => metrics.CurrentConcurrentExecutionCount));

            // error percentage derived from current metrics
            monitors.Add(getCurrentValueMonitor("errorPercentage", () => metrics.GetHealthCounts().ErrorPercentage));

            // execution latency metrics
            monitors.Add(getExecutionLatencyMeanMonitor("latencyExecute_mean"));
            monitors.Add(getExecutionLatencyPercentileMonitor("latencyExecute_percentile_5", 5));
            monitors.Add(getExecutionLatencyPercentileMonitor("latencyExecute_percentile_25", 25));
            monitors.Add(getExecutionLatencyPercentileMonitor("latencyExecute_percentile_50", 50));
            monitors.Add(getExecutionLatencyPercentileMonitor("latencyExecute_percentile_75", 75));
            monitors.Add(getExecutionLatencyPercentileMonitor("latencyExecute_percentile_90", 90));
            monitors.Add(getExecutionLatencyPercentileMonitor("latencyExecute_percentile_99", 99));
            monitors.Add(getExecutionLatencyPercentileMonitor("latencyExecute_percentile_995", 99.5));

            //            // total latency metrics
            monitors.Add(getTotalLatencyMeanMonitor("latencyTotal_mean"));
            monitors.Add(getTotalLatencyPercentileMonitor("latencyTotal_percentile_5", 5));
            monitors.Add(getTotalLatencyPercentileMonitor("latencyTotal_percentile_25", 25));
            monitors.Add(getTotalLatencyPercentileMonitor("latencyTotal_percentile_50", 50));
            monitors.Add(getTotalLatencyPercentileMonitor("latencyTotal_percentile_75", 75));
            monitors.Add(getTotalLatencyPercentileMonitor("latencyTotal_percentile_90", 90));
            monitors.Add(getTotalLatencyPercentileMonitor("latencyTotal_percentile_99", 99));
            monitors.Add(getTotalLatencyPercentileMonitor("latencyTotal_percentile_995", 995));

            // group
            //monitors.Add(ServoMetricFactory.InformationalMetric<string>(this,
            //    MonitorConfig.builder("commandGroup").build(),
            //    () => commandGroupKey != null ? commandGroupKey.Name : null));

            // properties (so the values can be inspected and monitored)
            monitors.Add(ServoMetricFactory.InformationalMetric<int>(this,
                MonitorConfig.builder("propertyValue_rollingStatisticalWindowInMilliseconds").build(),
                () => properties.MetricsRollingStatisticalWindowInMilliseconds.Get()));

            monitors.Add(ServoMetricFactory.InformationalMetric<int>(this,
                MonitorConfig.builder("propertyValue_circuitBreakerRequestVolumeThreshold").build(),
                () => properties.CircuitBreakerRequestVolumeThreshold.Get()));

            monitors.Add(ServoMetricFactory.InformationalMetric<long>(this,
                MonitorConfig.builder("propertyValue_circuitBreakerSleepWindowInMilliseconds").build(),
                () => properties.CircuitBreakerSleepWindow.Get().Milliseconds));

            monitors.Add(ServoMetricFactory.InformationalMetric<int>(this,
                MonitorConfig.builder("propertyValue_circuitBreakerErrorThresholdPercentage").build(),
                () => properties.CircuitBreakerErrorThresholdPercentage.Get()));

            monitors.Add(ServoMetricFactory.InformationalMetric<bool>(this,
                MonitorConfig.builder("propertyValue_circuitBreakerForceOpen").build(),
                () => properties.CircuitBreakerForceOpen.Get()));

            monitors.Add(ServoMetricFactory.InformationalMetric<bool>(this,
                MonitorConfig.builder("propertyValue_circuitBreakerForceClosed").build(),
                () => properties.CircuitBreakerForceClosed.Get()));

            monitors.Add(ServoMetricFactory.InformationalMetric<long>(this,
                MonitorConfig.builder("propertyValue_executionIsolationThreadTimeoutInMilliseconds").build(),
                () => properties.ExecutionIsolationThreadTimeout.Get().Milliseconds));

            //monitors.Add(ServoMetricFactory.InformationalMetric<TimeSpan>(this,
            //    MonitorConfig.builder("propertyValue_executionTimeoutInMilliseconds").build(),
            //    () => properties.executionTimeoutInMilliseconds.Get()));

            monitors.Add(ServoMetricFactory.InformationalMetric<ExecutionIsolationStrategy>(this,
                MonitorConfig.builder("propertyValue_executionIsolationStrategy").build(),
                () => properties.ExecutionIsolationStrategy.Get()));

            monitors.Add(ServoMetricFactory.InformationalMetric<bool>(this,
                MonitorConfig.builder("propertyValue_metricsRollingPercentileEnabled").build(),
                () => properties.MetricsRollingPercentileEnabled.Get()));

            monitors.Add(ServoMetricFactory.InformationalMetric<bool>(this,
                MonitorConfig.builder("propertyValue_requestCacheEnabled").build(),
                () => properties.RequestCacheEnabled.Get()));

            monitors.Add(ServoMetricFactory.InformationalMetric<bool>(this,
                MonitorConfig.builder("propertyValue_requestLogEnabled").build(),
                () => properties.RequestLogEnabled.Get()));

            monitors.Add(ServoMetricFactory.InformationalMetric<int>(this,
               MonitorConfig.builder("propertyValue_executionIsolationSemaphoreMaxConcurrentRequests").build(),
               () => properties.ExecutionIsolationSemaphoreMaxConcurrentRequests.Get()));

            monitors.Add(ServoMetricFactory.InformationalMetric<int>(this,
                MonitorConfig.builder("propertyValue_fallbackIsolationSemaphoreMaxConcurrentRequests").build(),
                () => properties.FallbackIsolationSemaphoreMaxConcurrentRequests.Get()));

            return monitors;
        }

        protected IMonitor<T> getCurrentValueMonitor<T>(string name, Func<T> metricToEvaluate)
        {
            return ServoMetricFactory.GaugeMetric<T>(this, MonitorConfig.builder(name).build(), metricToEvaluate);
        }

        protected IMonitor<T> getCurrentValueMonitor<T>(string name, Func<T> metricToEvaluate, ITag tag)
        {
            return ServoMetricFactory.GaugeMetric<T>(this, MonitorConfig.builder(name).withTag(tag).build(), metricToEvaluate);
        }

        protected IMonitor<long> getCumulativeMonitor(string name, HystrixEventType @event)
        {
            return ServoMetricFactory.CounterMetric<long>(this,
                MonitorConfig.builder(name).withTag(ServoTypeTag).withTag(ServoInstanceTag).build(),
                () => metrics.GetCumulativeCount(@event.ToHystrixRollingNumberEvent()));
        }

        protected IMonitor<long> getRollingMonitor(string name, HystrixEventType @event)
        {
            return ServoMetricFactory.GaugeMetric<long>(this,
                MonitorConfig.builder(name).withTag(DataSourceLevel.DEBUG).withTag(ServoTypeTag).withTag(ServoInstanceTag).build(),
                () => metrics.GetRollingCount(@event.ToHystrixRollingNumberEvent()));
        }

        protected IMonitor<int> getExecutionLatencyMeanMonitor(string name)
        {
            return ServoMetricFactory.GaugeMetric<int>(this,
                MonitorConfig.builder(name).build(),
                () => metrics.GetExecutionTimeMean());
        }

        protected IMonitor<int> getExecutionLatencyPercentileMonitor(string name, double percentile)
        {
            return ServoMetricFactory.GaugeMetric<int>(this,
                MonitorConfig.builder(name).build(),
                () => metrics.GetExecutionTimePercentile(percentile));
        }

        protected IMonitor<int> getTotalLatencyMeanMonitor(string name)
        {
            return ServoMetricFactory.GaugeMetric<int>(this,
                MonitorConfig.builder(name).build(),
                () => metrics.GetTotalTimeMean());
        }

        protected IMonitor<int> getTotalLatencyPercentileMonitor(string name, double percentile)
        {
            return ServoMetricFactory.GaugeMetric<int>(this,
                MonitorConfig.builder(name).build(),
                () => metrics.GetTotalTimePercentile(percentile));
        }
    }


    public static class asd
    {
        public static HystrixRollingNumberEvent ToHystrixRollingNumberEvent(this HystrixEventType eventType)
        {
            switch (eventType)
            {
                case HystrixEventType.Success: return HystrixRollingNumberEvent.Success;
                case HystrixEventType.Failure: return HystrixRollingNumberEvent.Failure;
                case HystrixEventType.Timeout: return HystrixRollingNumberEvent.Timeout;
                case HystrixEventType.ShortCircuited: return HystrixRollingNumberEvent.ShortCircuited;
                case HystrixEventType.ThreadPoolRejected: return HystrixRollingNumberEvent.ThreadPoolRejected;
                case HystrixEventType.SemaphoreRejected: return HystrixRollingNumberEvent.SemaphoreRejected;
                case HystrixEventType.FallbackSuccess: return HystrixRollingNumberEvent.FallbackSuccess;
                case HystrixEventType.FallbackFailure: return HystrixRollingNumberEvent.FallbackFailure;
                case HystrixEventType.FallbackRejection: return HystrixRollingNumberEvent.FallbackRejection;
                case HystrixEventType.ExceptionThrown: return HystrixRollingNumberEvent.ExceptionThrown;
                case HystrixEventType.ResponseFromCache: return HystrixRollingNumberEvent.ResponseFromCache;
                case HystrixEventType.Collapsed: return HystrixRollingNumberEvent.Collapsed;
                default: throw new Exception("Unknown HystrixEventType : " + eventType);
            }
        }
    }
}
