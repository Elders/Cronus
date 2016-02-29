using System;
using System.Collections.Generic;

namespace Elders.Cronus.IntegrityValidation
{
    public class IntegrityRule<T> : IEqualityComparer<IntegrityRule<T>>, IEquatable<IntegrityRule<T>>
    {
        public IntegrityRule(IValidator<T> validator, IResolver<T> resolver)
        {
            Resolver = resolver;
            Validator = validator;
        }

        public IValidator<T> Validator { get; }

        public IResolver<T> Resolver { get; }

        public bool Equals(IntegrityRule<T> left, IntegrityRule<T> right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left))
                return false;
            else
                return left.Equals(right);
        }

        public int GetHashCode(IntegrityRule<T> obj)
        {
            return obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            int startValue = 23;
            int multiplier = 77;

            int hashCode = startValue;

            hashCode = hashCode * multiplier ^ Validator.GetType().GetHashCode();
            hashCode = hashCode * multiplier ^ Resolver.GetType().GetHashCode();

            return hashCode;
        }

        public bool Equals(IntegrityRule<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            var t = GetType();
            if (t != other.GetType())
                return false;

            return
                Validator.GetType() == other.Validator.GetType() &&
                Resolver.GetType() == other.Resolver.GetType();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as IntegrityRule<T>);
        }

        public static bool operator ==(IntegrityRule<T> left, IntegrityRule<T> right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left))
                return false;
            else
                return left.Equals(right);
        }

        public static bool operator !=(IntegrityRule<T> left, IntegrityRule<T> right)
        {
            return !(left == right);

        }
    }
}