using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Cronus.Core.Eventing;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Core.Messaging
{
    public static class MessagingHelper
    {
        public static string GetPipelineName(object message)
        {
            return GetPipelineName(message.GetType());
        }

        public static string GetPipelineName(Type messageType)
        {
            var boundedContext = messageType.GetAssemblyAttribute<BoundedContextAttribute>();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly cointaining message type '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", messageType.FullName));

            if (messageType.GetInterfaces().Any(i => i == typeof(ICommand)))
                return boundedContext.CommandsPipelineName;
            else if (messageType.GetInterfaces().Any(i => i == typeof(IEvent)))
                return boundedContext.EventsPipelineName;
            else if (messageType.GetInterfaces().Any(i => i == typeof(IMessage)))
                return boundedContext.EventStorePipelineName;
            else
                throw new Exception(String.Format("The message of type '{0}' does not implement '{1}' or '{2}'", messageType.FullName, typeof(ICommand).FullName, typeof(IEvent).FullName));
        }
        public static string GetEventStorePipelineName(Assembly assembly)
        {
            var boundedContext = assembly.GetAssemblyAttribute<BoundedContextAttribute>();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", assembly.FullName));

            return boundedContext.EventStorePipelineName;
        }

        public static string GetCommandsPipelineName(Assembly assembly)
        {
            var boundedContext = assembly.GetAssemblyAttribute<BoundedContextAttribute>();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", assembly.FullName));

            return boundedContext.CommandsPipelineName;
        }

        public static string GetHandlerQueueName(Type messageType)
        {
            var boundedContext = messageType.GetAssemblyAttribute<BoundedContextAttribute>();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly containing message type '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", messageType.FullName));

            return String.Format("{0}.{1}", boundedContext.BoundedContextNamespace, messageType.Name);
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
        public static string GetMessageId(object message)
        {
            return GetMessageId(message.GetType());
        }

        public static string GetMessageId(Type messageType)
        {
            DataContractAttribute contract = messageType.GetCustomAttributes(false).Where(attr => attr is DataContractAttribute).SingleOrDefault() as DataContractAttribute;

            if (contract == null || String.IsNullOrEmpty(contract.Name))
                throw new Exception(String.Format(@"The message type '{0}' is missing a DataContract attribute. Example: [DataContract(""00000000-0000-0000-0000-000000000000"")]", messageType.FullName));

            return contract.Name;
        }

        private static IEnumerable<T> GetCustomAttributesIncludingBaseInterfaces<T>(this Type type)
        {
            var attributeType = typeof(T);
            return type
                .GetCustomAttributes(attributeType, true)
                .Union(type.GetInterfaces().SelectMany(interfaceType => interfaceType.GetCustomAttributes(attributeType, true)))
                .Distinct()
                .Cast<T>();
        }

        private static T GetAssemblyAttribute<T>(this Type type)
        {
            return GetAssemblyAttribute<T>(type.Assembly);
        }

        private static T GetAssemblyAttribute<T>(this Assembly assembly)
        {
            var attributeType = typeof(T);
            var attribute = assembly
                .GetCustomAttributes(attributeType, false)
                .SingleOrDefault();

            return attribute == null
                ? default(T)
                : (T)attribute;
        }

    }
}
