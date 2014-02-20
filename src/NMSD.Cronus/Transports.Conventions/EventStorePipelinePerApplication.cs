using System;
using System.Reflection;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Transports.Conventions
{
    public class EventStorePipelinePerApplication : IPipelineNameConvention
    {
        public string GetPipelineName(Type messageType)
        {
            var messageAssembly = messageType.Assembly;
            return GetPipelineName(messageAssembly);
        }

        private string GetPipelineName(Assembly assemblyContainingEvents)
        {
            var boundedContext = assemblyContainingEvents.GetAssemblyAttribute<BoundedContextAttribute>();
            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", assemblyContainingEvents.FullName));

            return boundedContext.EventStorePipelineName;
        }

    }
}