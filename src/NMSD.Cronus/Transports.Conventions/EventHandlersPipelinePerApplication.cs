using System;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Transports.Conventions
{
    public class EventHandlersPipelinePerApplication : IPipelineNameConvention
    {
        public string GetPipelineName(Type messageType)
        {
            var boundedContext = messageType.GetBoundedContext();
            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", messageType.Assembly.FullName));

            return boundedContext.EventsPipelineName;
        }
    }
}
