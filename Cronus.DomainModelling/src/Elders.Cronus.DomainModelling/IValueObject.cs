using System;
using System.Collections.Generic;
using System.Reflection;

namespace Elders.Cronus.DomainModelling
{
    public interface IValueObject<T> : IEqualityComparer<T>, IEquatable<T>
    {

    }
    /// <summary>
    /// The class which implements ValueObject<T> have to be marked with sealed keyword.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValueObject<T> : IValueObject<T> where T : ValueObject<T>
    {
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as T);
        }

        public override int GetHashCode()
        {
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            int startValue = 23;
            int multiplier = 77;

            int hashCode = startValue;

            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(this);

                if (value != null)
                    hashCode = hashCode * multiplier ^ value.GetHashCode();
            }
            return hashCode;
        }



        public virtual bool Equals(T other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            var t = GetType();
            if (t != other.GetType())
                return false;

            FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                object value1 = field.GetValue(other);
                object value2 = field.GetValue(this);

                if (ReferenceEquals(null, value1))
                {
                    if (!ReferenceEquals(null, value2))
                        return false;
                }
                else if (!value1.Equals(value2))
                    return false;
            }
            return true;
        }

        public static bool operator ==(ValueObject<T> x, ValueObject<T> y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ValueObject<T> x, ValueObject<T> y)
        {
            return !(x == y);

        }

        public bool Equals(T left, T right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left))
                return false;
            else
                return left.Equals(right);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
