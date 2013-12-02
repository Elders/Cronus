using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
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
            BoundedContextAttribute contract = messageType.GetCustomAttributesIncludingBaseInterfaces<BoundedContextAttribute>().SingleOrDefault();

            if (contract == null)
                throw new Exception(String.Format(@"The message type '{0}' is missing a BoundedContext attribute. Example: [BoundedContext(""Company.Product.BoundedContext"")]", messageType.FullName));

            return contract.PipelineName;
        }

        public static string GetQueuePrefix(Type messageType)
        {
            BoundedContextAttribute contract = messageType.GetCustomAttributesIncludingBaseInterfaces<BoundedContextAttribute>().SingleOrDefault();

            if (contract == null)
                throw new Exception(String.Format(@"The message type '{0}' is missing a BoundedContext attribute. Example: [BoundedContext(""Company.Product.BoundedContext"")]", messageType.FullName));

            return contract.BoundedContextNamespace;
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



        /// <summary>Searches and returns attributes. The inheritance chain is not used to find the attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this Type type) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), false).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Searches and returns attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes. Interfaces will be searched, too.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this Type type, bool inherit) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), inherit).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Private helper for searching attributes.</summary>
        /// <param name="type">The type which is searched for the attribute.</param>
        /// <param name="attributeType">The type of attribute to search for.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attribute. Interfaces will be searched, too.</param>
        /// <returns>An array that contains all the custom attributes, or an array with zero elements if no attributes are defined.</returns>
        private static object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
            if (!inherit)
            {
                return type.GetCustomAttributes(attributeType, false);
            }

            var attributeCollection = new Collection<object>();
            var baseType = type;

            do
            {
                baseType.GetCustomAttributes(attributeType, true).Apply(attributeCollection.Add);
                baseType = baseType.BaseType;
            }
            while (baseType != null);

            foreach (var interfaceType in type.GetInterfaces())
            {
                GetCustomAttributes(interfaceType, attributeType, true).Apply(attributeCollection.Add);
            }

            var attributeArray = new object[attributeCollection.Count];
            attributeCollection.CopyTo(attributeArray, 0);
            return attributeArray;
        }

        /// <summary>Applies a function to every element of the list.</summary>
        private static void Apply<T>(this IEnumerable<T> enumerable, Action<T> function)
        {
            foreach (var item in enumerable)
            {
                function.Invoke(item);
            }
        }
    }
}
