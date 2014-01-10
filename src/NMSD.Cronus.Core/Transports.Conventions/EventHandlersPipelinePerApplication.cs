using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Core.Transports.Conventions
{
    public class EventHandlersPipelinePerApplication : IEventHandlersPipelineConvention
    {
        public string GetPipelineName(Type messageType)
        {
            var assemblyContainingEvents = messageType.Assembly;
            var boundedContext = assemblyContainingEvents.GetAssemblyAttribute<BoundedContextAttribute>();
            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", assemblyContainingEvents.FullName));

            return boundedContext.EventsPipelineName;
        }
    }
}
