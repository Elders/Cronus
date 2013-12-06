using System;
using System.Text;

namespace NMSD.Cronus.Core.Cqrs
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class BoundedContextAttribute : Attribute
    {
        private string boundedContextName;

        private string boundedContextNamespace;

        private string companyName;

        private string commandsPipelineName;

        private string eventsPipelineName;

        private string productName;
        
        private string systemPipelineName;

        public BoundedContextAttribute(string boundedContextNamespace)
        {
            this.boundedContextNamespace = boundedContextNamespace;
            string[] splitted = boundedContextNamespace.Split('.');
            this.companyName = splitted[0];
            StringBuilder productNameBuilder = new StringBuilder();  // Replace with regex
            for (int i = 1; i < splitted.Length - 1; i++)
            {
                productNameBuilder.Append(splitted[i]);
                productNameBuilder.Append('.');
            }
            this.productName = productNameBuilder.ToString().TrimEnd('.');
            this.boundedContextName = splitted[splitted.Length - 1];
            this.commandsPipelineName = String.Format("{0}.{1}.Commands", companyName, productName);
            this.eventsPipelineName = String.Format("{0}.{1}.Events", companyName, productName);
            this.systemPipelineName = String.Format("{0}.{1}.System", companyName, productName);
        }

        public BoundedContextAttribute(string companyName, string productName, string boundedContextName)
        {
            this.boundedContextName = boundedContextName;
            this.productName = productName;
            this.companyName = companyName;
            this.boundedContextNamespace = String.Format("{0}.{1}.{2}", companyName, productName, boundedContextName);
            this.commandsPipelineName = String.Format("{0}.{1}.Commands", companyName, productName);
            this.eventsPipelineName = String.Format("{0}.{1}.Events", companyName, productName);
            this.systemPipelineName = String.Format("{0}.{1}.System", companyName, productName);
        }

        public string BoundedContextName { get { return boundedContextName; } }

        public string BoundedContextNamespace { get { return boundedContextNamespace; } }

        public string CompanyName { get { return companyName; } }

        public string CommandsPipelineName { get { return commandsPipelineName; } }

        public string EventsPipelineName { get { return eventsPipelineName; } }

        public string SystemPipelineName { get { return systemPipelineName; } }

        public string ProductName { get { return productName; } }

    }
}
