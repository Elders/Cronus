using System;
using System.Globalization;
using System.Threading;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Userfull;
using Microsoft.Extensions.Caching.Memory;

namespace Elders.Cronus.AtomicAction.InMemory
{
    public class InMemoryAggregateRootAtomicAction : IAggregateRootAtomicAction
    {
        readonly MemoryCache aggregateLock = null;
        readonly MemoryCache aggregateRevisions = null;
        readonly MemoryCacheEntryOptions cacheEntryOptions;

        public InMemoryAggregateRootAtomicAction()
        {
            var aggregateLockMemoryCacheOptions = new MemoryCacheOptions()
            {
                SizeLimit = 500 * 1024 * 1024, //500mb
                CompactionPercentage = 0.1, // 10%
                ExpirationScanFrequency = new TimeSpan(0, 1, 0)
            };

            var aggregateRevisionsMemoryCacheOptions = new MemoryCacheOptions()
            {
                SizeLimit = 500 * 1024 * 1024, //500mb
                CompactionPercentage = 0.1, // 10%
                ExpirationScanFrequency = new TimeSpan(0, 1, 0)
            };

            cacheEntryOptions = new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromSeconds(30d),
                Size = 1
            };

            aggregateLock = new MemoryCache(aggregateLockMemoryCacheOptions);
            aggregateRevisions = new MemoryCache(aggregateRevisionsMemoryCacheOptions);
        }

        public Result<bool> Execute(IAggregateRootId aggregateRootId, int aggregateRootRevision, Action action)
        {
            var result = new Result<bool>(false);
            var acquired = new AtomicBoolean(false);

            try
            {
                acquired = aggregateLock.Get(aggregateRootId.Urn.Value) as AtomicBoolean;
                if (ReferenceEquals(null, acquired))
                {
                    acquired = aggregateLock.Set(aggregateRootId.Urn.Value, new AtomicBoolean(false), cacheEntryOptions);
                    if (ReferenceEquals(null, acquired))
                        return result;
                }

                if (acquired.CompareAndSet(false, true))
                {
                    try
                    {
                        AtomicInteger revision = aggregateRevisions.Get(aggregateRootId.Urn.Value) as AtomicInteger;
                        if (ReferenceEquals(null, revision))
                        {
                            var newRevision = new AtomicInteger(aggregateRootRevision - 1);
                            revision = aggregateRevisions.Set(aggregateRootId.Urn.Value, newRevision, cacheEntryOptions);
                            if (ReferenceEquals(null, revision))
                                return result;
                        }

                        var currentRevision = revision.Value;
                        if (revision.CompareAndSet(aggregateRootRevision - 1, aggregateRootRevision))
                        {
                            try
                            {
                                action();
                                return Result.Success;
                            }
                            catch (Exception)
                            {
                                revision.GetAndSet(currentRevision);
                                throw;
                            }
                        }
                    }
                    finally
                    {
                        acquired.GetAndSet(false);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return result.WithError(ex);
            }
        }

        public void Dispose()
        {
            aggregateRevisions?.Dispose();
            aggregateLock?.Dispose();
        }
    }

    [Serializable]
    public class AtomicBoolean : IFormattable
    {
        private volatile int booleanValue;

        // <summary>
        // Gets or sets the current value.
        // </summary>
        public bool Value
        {
            get { return this.booleanValue != 0; }
            set { this.booleanValue = value ? 1 : 0; }
        }

        public AtomicBoolean()
            : this(false)
        {
        }
        public AtomicBoolean(bool initialValue)
        {
            Value = initialValue;
        }

        public bool CompareAndSet(bool expect, bool update)
        {
            int expectedIntValue = expect ? 1 : 0;
            int newIntValue = update ? 1 : 0;
            return Interlocked.CompareExchange(ref this.booleanValue, newIntValue, expectedIntValue) == expectedIntValue;
        }
        public bool GetAndSet(bool newValue)
        {
            return Interlocked.Exchange(ref this.booleanValue, newValue ? 1 : 0) != 0;
        }
        public bool WeakCompareAndSet(bool expect, bool update)
        {
            return CompareAndSet(expect, update);
        }

        public override bool Equals(object obj)
        {
            return obj as AtomicBoolean == this;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return ToString(CultureInfo.CurrentCulture);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return Value.ToString(formatProvider);
        }

        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(formatProvider);
        }

        public static bool operator ==(AtomicBoolean left, AtomicBoolean right)
        {
            if (Object.ReferenceEquals(left, null) || Object.ReferenceEquals(right, null))
                return false;

            return left.Value == right.Value;
        }

        public static bool operator !=(AtomicBoolean left, AtomicBoolean right)
        {
            return !(left == right);
        }

        public static implicit operator bool(AtomicBoolean atomic)
        {
            if (atomic == null) { return false; }
            else { return atomic.Value; }
        }
    }

    [Serializable]
    public class AtomicInteger : IFormattable
    {
        private volatile int integerValue;

        // <summary>
        // Gets or sets the current value.
        // </summary>
        public int Value
        {
            get { return this.integerValue; }
            set { this.integerValue = value; }
        }

        public AtomicInteger()
            : this(0)
        {
        }
        public AtomicInteger(int initialValue)
        {
            this.integerValue = initialValue;
        }

        public int AddAndGet(int delta)
        {
            return Interlocked.Add(ref this.integerValue, delta);
        }
        public bool CompareAndSet(int expect, int update)
        {
            return Interlocked.CompareExchange(ref this.integerValue, update, expect) == expect;
        }
        public int DecrementAndGet()
        {
            return Interlocked.Decrement(ref this.integerValue);
        }
        public int GetAndDecrement()
        {
            return Interlocked.Decrement(ref this.integerValue) + 1;
        }
        public int GetAndIncrement()
        {
            return Interlocked.Increment(ref this.integerValue) - 1;
        }
        public int GetAndSet(int newValue)
        {
            return Interlocked.Exchange(ref this.integerValue, newValue);
        }
        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref this.integerValue);
        }
        public bool WeakCompareAndSet(int expect, int update)
        {
            return CompareAndSet(expect, update);
        }

        public override bool Equals(object obj)
        {
            return obj as AtomicInteger == this;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public override string ToString()
        {
            return ToString(CultureInfo.CurrentCulture);
        }
        public string ToString(IFormatProvider formatProvider)
        {
            return Value.ToString(formatProvider);
        }
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(formatProvider);
        }

        public static bool operator ==(AtomicInteger left, AtomicInteger right)
        {
            if (Object.ReferenceEquals(left, null) || Object.ReferenceEquals(right, null))
                return false;

            return left.Value == right.Value;
        }
        public static bool operator !=(AtomicInteger left, AtomicInteger right)
        {
            return !(left == right);
        }
        public static implicit operator int(AtomicInteger atomic)
        {
            if (atomic == null)
            {
                return 0;
            }
            else
            {
                return atomic.Value;
            }
        }
    }
}
