using System;
using System.Globalization;
using System.Text;

namespace NMSD.Cronus.Core.DomainModelling
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class BoundedContextAttribute : Attribute
    {
        private string boundedContextName;

        private string boundedContextNamespace;

        private string companyName;

        private string commandsPipelineName;

        private string eventsPipelineName;

        private string productName;

        private string eventStorePipelineName;

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
            this.commandsPipelineName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.Commands", companyName, productName);
            this.eventsPipelineName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.Events", companyName, productName);
            this.eventStorePipelineName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.EventStore", companyName, productName);
        }

        public BoundedContextAttribute(string companyName, string productName, string boundedContextName)
        {
            this.boundedContextName = boundedContextName;
            this.productName = productName;
            this.companyName = companyName;
            this.boundedContextNamespace = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", companyName, productName, boundedContextName);
            this.commandsPipelineName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.Commands", companyName, productName);
            this.eventsPipelineName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.Events", companyName, productName);
            this.eventStorePipelineName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.EventStore", companyName, productName);
        }

        public string BoundedContextName { get { return boundedContextName; } }

        public string BoundedContextNamespace { get { return boundedContextNamespace; } }

        public string CompanyName { get { return companyName; } }

        public string CommandsPipelineName { get { return commandsPipelineName; } }

        public string EventsPipelineName { get { return eventsPipelineName; } }

        public string EventStorePipelineName { get { return eventStorePipelineName; } }

        public string ProductName { get { return productName; } }

    }
}
