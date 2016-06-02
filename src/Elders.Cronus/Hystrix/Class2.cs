using System;
using Netflix.Servo.Attributes;
using Netflix.Servo.Monitor;
using Netflix.Servo.Tag;

namespace Hystrix.Contrib.ServoPublisher
{
    public abstract class HystrixServoMetricsPublisherAbstract
    {
        protected abstract ITag ServoTypeTag { get; }

        protected abstract ITag ServoInstanceTag { get; }

        internal class InformationalServoMetric<T> : AbstractMonitor<T>
        {
            readonly Func<T> getValue;

            public InformationalServoMetric(MonitorConfig config, HystrixServoMetricsPublisherAbstract publisher, Func<T> getValue)
                : base(config
                      .withAdditionalTag(DataSourceType.INFORMATIONAL)
                      .withAdditionalTag(publisher.ServoTypeTag)
                      .withAdditionalTag(publisher.ServoInstanceTag))
            {
                this.getValue = getValue;
            }

            public override T GetValue() { return getValue(); }
            public override T GetValue(int pollerIndex) { return GetValue(); }
        }

        internal class GaugeServoMetric<T> : AbstractMonitor<T>, IGauge<T>
        {
            readonly Func<T> getValue;

            public GaugeServoMetric(MonitorConfig config, HystrixServoMetricsPublisherAbstract publisher, Func<T> getValue)
                : base(config
                      .withAdditionalTag(DataSourceType.GAUGE)
                      .withAdditionalTag(publisher.ServoTypeTag)
                      .withAdditionalTag(publisher.ServoInstanceTag))
            {
                this.getValue = getValue;
            }

            public override T GetValue() { return getValue(); }
            public override T GetValue(int pollerIndex) { return GetValue(); }
        }

        internal class CounterServoMetric<T> : AbstractMonitor<T>, ICounter<T>
        {
            readonly Func<T> getValue;

            public CounterServoMetric(MonitorConfig config, HystrixServoMetricsPublisherAbstract publisher, Func<T> getValue)
                : base(config
                      .withAdditionalTag(DataSourceType.COUNTER)
                      .withAdditionalTag(publisher.ServoTypeTag)
                      .withAdditionalTag(publisher.ServoInstanceTag))
            {
                this.getValue = getValue;
            }

            public override T GetValue() { return getValue(); }
            public override T GetValue(int pollerIndex) { return GetValue(); }

            public void increment() { throw new InvalidOperationException("We are wrapping a value instead."); }
            public void increment(long amount) { throw new InvalidOperationException("We are wrapping a value instead."); }
        }
    }

    public static class ServoMetricFactory
    {
        public static IMonitor<T> GaugeMetric<T>(HystrixServoMetricsPublisherAbstract self, MonitorConfig config, Func<T> getValue)
        {
            var monitor = new HystrixServoMetricsPublisherAbstract.GaugeServoMetric<T>(config, self, getValue);
            return monitor;
        }

        public static IMonitor<T> CounterMetric<T>(HystrixServoMetricsPublisherAbstract self, MonitorConfig config, Func<T> getValue)
        {
            var monitor = new HystrixServoMetricsPublisherAbstract.CounterServoMetric<T>(config, self, getValue);
            return monitor;
        }

        public static IMonitor<T> InformationalMetric<T>(HystrixServoMetricsPublisherAbstract self, MonitorConfig config, Func<T> getValue)
        {
            var monitor = new HystrixServoMetricsPublisherAbstract.InformationalServoMetric<T>(config, self, getValue);
            return monitor;
        }
    }
}
