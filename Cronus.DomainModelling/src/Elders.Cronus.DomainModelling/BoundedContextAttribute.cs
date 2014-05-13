using System;
using System.Globalization;
using System.Text;

namespace Elders.Cronus.DomainModelling
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class BoundedContextAttribute : Attribute
    {
        private string boundedContextName;

        private string boundedContextNamespace;

        private string companyName;

        private string productName;

        private string productNamespace;

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
            this.productNamespace = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", companyName, productName);
        }

        public BoundedContextAttribute(string companyName, string productName, string boundedContextName)
        {
            this.boundedContextName = boundedContextName;
            this.productName = productName;
            this.companyName = companyName;
            this.boundedContextNamespace = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", companyName, productName, boundedContextName);
            this.productNamespace = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", companyName, productName);
        }

        public string BoundedContextName { get { return boundedContextName; } }

        public string BoundedContextNamespace { get { return boundedContextNamespace; } }

        public string CompanyName { get { return companyName; } }

        public string ProductName { get { return productName; } }

        public string ProductNamespace { get { return productNamespace; } }

    }
}
