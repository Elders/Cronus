using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NSMD.Cronus.RabbitMQ.Transports.Conventions
{
    public interface IEndpointConvention
    {
        EndpointDefinition GetEndpointDefinition(Type handledMessageType, Type handlerType);
    }
    public interface IPipelineConvention
    {
        string GetPipelineName();
    }

    public class EndpointDefinition
    {
        public string EndpointName { get; private set; }

        public List<object> EndpointMessagesDefinitions { get; private set; }

        public EndpointDefinition()
        {

        }
    }
    public class BoundedContextAttribute
    {
        public string BoundedContextNamespace { get; set; }
    }
    public static class ReflectionExtensions
    {
        public static T GetAssemblyAttribute<T>(this Type type)
        {
            return GetAssemblyAttribute<T>(type.Assembly);
        }

        public static T GetAssemblyAttribute<T>(this Assembly assembly)
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

    public class EndpointPerHandler : IEndpointConvention
    {
        public EndpointDefinition GetEndpointDefinition(Type handledMessageType, Type handlerType)
        {
            var boundedContext = handlerType.GetAssemblyAttribute<BoundedContextAttribute>();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly containing message type '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", handlerType.FullName));

            string endpointName = String.Format("{0}.{1}", boundedContext.BoundedContextNamespace, handlerType.Name);

            handlerType.GetMethods(BindingFlags.)
        }
    }

    public class EndpointPerBoundedContext : IEndpointConvention
    {
        public EndpointPerBoundedContext(Type handlerType)
        {

        }
        public EndpointDefinition GetEndpointDefinition()
        {
            return null;
        }
    }
}
