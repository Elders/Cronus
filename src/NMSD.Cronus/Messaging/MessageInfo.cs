using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;
namespace NMSD.Cronus
{
    public static class MessageInfo
    {
        private static readonly ConcurrentDictionary<Type, Guid> boundedContextIdsForType = new ConcurrentDictionary<Type, Guid>();
        private static readonly ConcurrentDictionary<Guid, BoundedContextAttribute> boundedContexts = new ConcurrentDictionary<Guid, BoundedContextAttribute>();

        public static BoundedContextAttribute GetBoundedContext(this Type messageType)
        {
            Guid boundedContextId;
            if (boundedContextIdsForType.TryGetValue(messageType, out boundedContextId))
            {
                return boundedContexts[boundedContextId];
            }
            else
            {
                var boundedContext = messageType.Assembly.GetAssemblyAttribute<BoundedContextAttribute>();
                boundedContextId = Guid.NewGuid();
                boundedContexts.TryAdd(boundedContextId, boundedContext);
                boundedContextIdsForType.TryAdd(messageType, boundedContextId);
                return boundedContext;
            }
        }

        public static string ToString(this IMessage message, string info, params object[] args)
        {
            var bcNamespace = GetBoundedContextNamespace(message.GetType());
            var messageInfo = String.Format(info, args);
            return String.Format("[{0}] {1}.", bcNamespace, messageInfo);
        }

        public static string GetMessageId(this IMessage message, string info, params object[] args)
        {
            var bcNamespace = GetBoundedContextNamespace(message.GetType());
            var messageInfo = String.Format(info, args);
            return String.Format("[{0}] {1}.", bcNamespace, messageInfo);
        }

        public static string GetMessageId(Type messageType)
        {
            DataContractAttribute contract = messageType.GetCustomAttributes(false).Where(attr => attr is DataContractAttribute).SingleOrDefault() as DataContractAttribute;

            if (contract == null || String.IsNullOrEmpty(contract.Name))
                throw new Exception(String.Format(@"The message type '{0}' is missing a DataContract attribute. Example: [DataContract(""00000000-0000-0000-0000-000000000000"")]", messageType.FullName));

            return contract.Name;
        }

        public static string GetBoundedContextName(Type messageType)
        {
            var boundedContext = messageType.GetBoundedContext();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly containing message type '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", messageType.FullName));

            return boundedContext.BoundedContextName;
        }
        //public static string GetBoundedContext(Assembly assembly)
        //{
        //    var boundedContext = assembly.GetAssemblyAttribute<BoundedContextAttribute>();

        //    if (boundedContext == null)
        //        throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", assembly.FullName));

        //    return boundedContext.BoundedContextName;
        //}

        public static string GetBoundedContextNamespace(Type messageType)
        {
            var boundedContext = messageType.GetBoundedContext();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly containing message type '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", messageType.FullName));

            return boundedContext.BoundedContextNamespace;
        }

        public static string GetBoundedContextNamespace(Assembly assembly)
        {
            var boundedContext = assembly.GetAssemblyAttribute<BoundedContextAttribute>();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", assembly.FullName));

            return boundedContext.BoundedContextNamespace;
        }
    }
}
