using System;
using System.Collections.Generic;
using Elders.Cronus.Pipeline.Hosts.DisposableExtensions;
using Elders.Cronus.Logging;

namespace Elders.Cronus.Pipeline.Hosts
{
    public class CronusHost : ICronusHost
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(CronusHost));

        public CronusHost()
        {
            Consumers = new List<ICronusConsumer>();
        }

        public List<ICronusConsumer> Consumers { get; set; }

        public bool Start()
        {
            foreach (var consumer in Consumers)
            {
                consumer.Start();
            }
            log.Info("Cronus hosts started succesfully.");
            return true;
        }

        public bool Stop()
        {
            foreach (var consumer in Consumers)
            {
                consumer.Stop();
            }
            Consumers.Clear();
            log.Info("Cronus hosts stopped succesfully.");
            return true;
        }

        public void Dispose()
        {
            Consumers.TryDisposeCollection(x => x);
        }
    }
}

namespace Elders.Cronus.Pipeline.Hosts.DisposableExtensions
{
    public static class IDisposableExtensions
    {
        public static void TryDispose<T>(this T @self)
        {
            if (ReferenceEquals(null, @self) == false)
            {
                var asDisposable = @self as IDisposable;
                if (asDisposable != null)
                    asDisposable.Dispose();
            }
        }
        public static void TryDisposeCollection<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source != null)
            {
                foreach (var es in source)
                {
                    var selected = keySelector(es);
                    selected.TryDispose();
                }
            }
        }
    }
}