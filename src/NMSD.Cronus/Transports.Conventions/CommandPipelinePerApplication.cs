using System;

namespace NMSD.Cronus.Transports.Conventions
{
    public class CommandPipelinePerApplication : IPipelineNameConvention
    {
        public string GetPipelineName(Type commandType)
        {
            var boundedContext = commandType.GetBoundedContext();
            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", commandType.Assembly.FullName));

            return boundedContext.CommandsPipelineName;
        }
    }
}
