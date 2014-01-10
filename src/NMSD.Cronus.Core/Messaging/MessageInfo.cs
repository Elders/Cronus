using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.DomainModelling;
using NMSD.Cronus.Core.Messaging;
namespace NMSD.Cronus.Core
{
    public static class MessageInfo
    {
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

        public static string GetBoundedContext(Type messageType)
        {
            var boundedContext = messageType.GetAssemblyAttribute<BoundedContextAttribute>();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly containing message type '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", messageType.FullName));

            return boundedContext.BoundedContextName;
        }
        public static string GetBoundedContext(Assembly assembly)
        {
            var boundedContext = assembly.GetAssemblyAttribute<BoundedContextAttribute>();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", assembly.FullName));

            return boundedContext.BoundedContextName;
        }

        public static string GetBoundedContextNamespace(Type messageType)
        {
            var boundedContext = messageType.GetAssemblyAttribute<BoundedContextAttribute>();

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
