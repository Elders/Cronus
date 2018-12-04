using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elders.Cronus.Multitenancy
{
    public class MockedConfiguration : IConfiguration
    {
        private readonly Dictionary<string, string> configurations;

        public MockedConfiguration()
        {
            configurations = new Dictionary<string, string>();
        }

        public string this[string key] { get => configurations[key]; set => configurations[key] = value; }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            throw new NotImplementedException();
        }
    }
}
