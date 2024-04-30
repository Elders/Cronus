using System;

namespace Elders.Cronus.AspNetCore.Exceptions;

[Serializable]
// Important: This attribute is NOT inherited from Exception, and MUST be specified 
// otherwise serialization will fail with a SerializationException stating that
// "Type X in Assembly Y is not marked as serializable."
public class UnableToResolveTenantException : Exception
{
    public UnableToResolveTenantException() { }

    public UnableToResolveTenantException(string message) : this(message, null) { }

    public UnableToResolveTenantException(string message, Exception innerException) : base(message, innerException) { }
}
