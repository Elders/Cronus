using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;

namespace NMSD.Cronus
{
    public static class MessageInfo
    {
        private static readonly ConcurrentDictionary<string, BoundedContextAttribute> boundedContexts = new ConcurrentDictionary<string, BoundedContextAttribute>();

        private static readonly ConcurrentDictionary<Type, string> contractIds = new ConcurrentDictionary<Type, string>();

        public static BoundedContextAttribute GetBoundedContext(this Type contractType)
        {
            BoundedContextAttribute boundedContext;
            string contractId;
            if (contractIds.TryGetValue(contractType, out contractId))
            {
                if (boundedContexts.TryGetValue(contractId, out boundedContext))
                    return boundedContext;
            }

            contractId = contractType.GetContractId();
            return contractType.GetAndCacheBoundedContextFromAttribute(contractId);
        }

        public static string GetContractId(this Type messageType)
        {
            string messageId;
            if (!contractIds.TryGetValue(messageType, out messageId))
            {
                messageId = GetAndCacheContractIdFromAttribute(messageType);
            }
            return messageId;
        }

        public static string ToString(this IMessage message, string info, params object[] args)
        {
            var bcNamespace = message.GetType().GetBoundedContext().BoundedContextNamespace;
            var messageInfo = String.Format(info, args);
            return "[" + bcNamespace + "] " + messageInfo;
        }

        private static BoundedContextAttribute GetAndCacheBoundedContextFromAttribute(this Type contractType, string contractId)
        {
            var boundedContext = contractType.Assembly.GetAssemblyAttribute<BoundedContextAttribute>();

            if (boundedContext == default(BoundedContextAttribute))
                throw new Exception(String.Format(@"The assembly containing message type '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", contractType.FullName));

            boundedContexts.TryAdd(contractId, boundedContext);
            return boundedContext;
        }

        private static string GetAndCacheContractIdFromAttribute(Type contractType)
        {
            string contractId;
            DataContractAttribute contract = contractType.GetCustomAttributes(false).Where(attr => attr is DataContractAttribute).SingleOrDefault() as DataContractAttribute;

            if (contract == null || String.IsNullOrEmpty(contract.Name))
            {
                if (typeof(IMessageHandler).IsAssignableFrom(contractType))
                    contractId = contractType.GetHashCode().ToString();
                else
                    throw new Exception(String.Format(@"The message type '{0}' is missing a DataContract attribute. Example: [DataContract(""00000000-0000-0000-0000-000000000000"")]", contractType.FullName));
            }
            else
            {
                contractId = contract.Name;
            }

            contractIds.TryAdd(contractType, contractId);
            return contractId;
        }

    }
}
