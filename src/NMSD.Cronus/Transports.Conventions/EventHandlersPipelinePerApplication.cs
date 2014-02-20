using System;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Transports.Conventions
{
    public class EventHandlersPipelinePerApplication : IPipelineNameConvention
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
