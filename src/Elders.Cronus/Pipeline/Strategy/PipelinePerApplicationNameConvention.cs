using System;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline.Transport.Strategy
{
    public class PipelinePerApplicationNameConvention : PipelineNameConvention
    {
        public override string GetPipelineName(Type messageType)
        {
            if (typeof(ICommand).IsAssignableFrom(messageType))
                return GetCommandsPipelineName(messageType);
            else if (typeof(IEvent).IsAssignableFrom(messageType))
                return GetEventsPipelineName(messageType);
            else if (typeof(IScheduledMessage).IsAssignableFrom(messageType))
                return GetEventsPipelineName(messageType);
            else
                throw new Exception(string.Format("The message type '{0}' is not eligible. Please use ICommand or IEvent", messageType));
        }

        protected override string GetCommandsPipelineName(Type messageType)
        {
            return GetBoundedContext(messageType).ProductNamespace + ".Commands";
        }

        protected override string GetEventsPipelineName(Type messageType)
        {
            return GetBoundedContext(messageType).ProductNamespace + ".Events";
        }

        private BoundedContextAttribute GetBoundedContext(Type messageType)
        {
            var boundedContext = messageType.GetBoundedContext();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", messageType.Assembly.FullName));
            return boundedContext;
        }
    }
}
