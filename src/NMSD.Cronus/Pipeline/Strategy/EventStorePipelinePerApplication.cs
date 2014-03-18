using System;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Pipeline.Strategy
{
    public class EventStorePipelinePerApplication : IPipelineNameConvention
    {
        public string GetPipelineName(Type messageType)
        {
            var boundedContext = messageType.GetBoundedContext();
            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", messageType.Assembly.FullName));

            return GetPipelineName(boundedContext);
        }

        public string GetPipelineName(BoundedContextAttribute boundedContext)
        {
            return boundedContext.ProductNamespace + ".EventStore";
        }
    }
}