using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;

namespace Elders.Cronus.DomainModeling
{
    public class InMemoryAggregateRootAtomicAction : IAggregateRootAtomicAction
    {
        static ConcurrentDictionary<IAggregateRootId, AtomicBoolean> aggregateLock = new ConcurrentDictionary<IAggregateRootId, AtomicBoolean>();
        static ConcurrentDictionary<IAggregateRootId, AtomicInteger> aggregateRevisions = new ConcurrentDictionary<IAggregateRootId, AtomicInteger>();

        public bool AtomicAction(IAggregateRootId aggregateRootId, int aggregateRootRevision, Action action, out Exception error)
        {
            error = null;
            AtomicBoolean acquired = new AtomicBoolean(false);

            try
            {
                if (!aggregateLock.TryGetValue(aggregateRootId, out acquired))
                {
                    acquired = acquired ?? new AtomicBoolean(false);
                    if (!aggregateLock.TryAdd(aggregateRootId, acquired))
                        aggregateLock.TryGetValue(aggregateRootId, out acquired);
                }

                if (acquired.CompareAndSet(false, true))
                {
                    try
                    {
                        AtomicInteger revision = null;
                        if (!aggregateRevisions.TryGetValue(aggregateRootId, out revision))
                        {
                            revision = new AtomicInteger(aggregateRootRevision - 1);
                            if (!aggregateRevisions.TryAdd(aggregateRootId, revision))
                                return false;
                        }
                        var currentRevision = revision.Value;
                        if (revision.CompareAndSet(aggregateRootRevision - 1, aggregateRootRevision))
                        {
                            try
                            {
                                action();
                                return true;
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

                return false;
            }
            catch (Exception ex)
            {
                error = ex;
                return false;
            }
        }

        public void Dispose()
        {
            if (aggregateLock != null)
                aggregateLock = null;
        }
    }

    [Serializable]
    public class AtomicBoolean : IFormattable
    {
        private volatile int booleanValue;

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
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

        public static implicit operator bool (AtomicBoolean atomic)
        {
            if (atomic == null) { return false; }
            else { return atomic.Value; }
        }
    }

    [Serializable]
    public class AtomicInteger : IFormattable
    {
        private volatile int integerValue;

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
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
        public static implicit operator int (AtomicInteger atomic)
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
