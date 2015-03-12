using System;

namespace Elders.Cronus.IocContainer
{
    /// <summary>
    /// Mapping key. See <see cref="IocContainer"/>
    /// </summary>
    internal class MappingKey
    {
        /// <summary>
        /// Type of the dependency
        /// </summary>
        public Type Type { get; protected set; }


        /// <summary>
        /// Name of the instance (optional)
        /// </summary>
        public string InstanceName { get; protected set; }


        /// <summary>
        /// Creates a new instance of <see cref="MappingKey"/>
        /// </summary>
        /// <param name="type">Type of the dependency</param>
        /// <param name="instanceName">Name of the instance</param>
        /// <exception cref="ArgumentNullException">type</exception>
        public MappingKey(Type type, string instanceName)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            Type = type;
            InstanceName = instanceName;
        }


        /// <summary>
        /// Returns the hash code for this instance
        /// </summary>
        /// <returns>The hash code for this instance</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                const int multiplier = 31;
                int hash = GetType().GetHashCode();

                hash = hash * multiplier + Type.GetHashCode();
                hash = hash * multiplier + (InstanceName == null ? 0 : InstanceName.GetHashCode());

                return hash;
            }
        }


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object</param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var compareTo = obj as MappingKey;

            if (ReferenceEquals(this, compareTo))
                return true;

            if (compareTo == null)
                return false;

            return Type == compareTo.Type &&
                string.Equals(InstanceName, compareTo.InstanceName, StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// For debugging purposes only
        /// </summary>
        /// <returns>Returns a string that represents the current object.</returns>
        public override string ToString()
        {
            const string format = "{0} ({1}) - hash code: {2}";

            return string.Format(format, InstanceName ?? "[null]",
                Type.FullName,
                GetHashCode()
            );
        }


        /// <summary>
        /// In case you need to return an error to the client program
        /// </summary>
        /// <returns>Trace string</returns>
        public string ToTraceString()
        {
            const string format = "Instance Name: {0} ({1})";

            return string.Format(format, InstanceName ?? "[null]", Type.FullName);
        }
    }
}
