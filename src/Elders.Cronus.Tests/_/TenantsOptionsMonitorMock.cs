using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Elders.Cronus
{
    public class TenantsOptionsMonitorMock : IOptionsMonitor<TenantsOptions>
    {
        private List<Action<TenantsOptions, string>> _listeners = new List<Action<TenantsOptions, string>>();
        private TenantsOptions _currentValue;

        public TenantsOptionsMonitorMock(params string[] tenants)
        {
            _currentValue = new TenantsOptions { Tenants = tenants };
        }

        public TenantsOptions CurrentValue => _currentValue;

        public TenantsOptions Get(string name)
        {
            return CurrentValue;
        }

        public IDisposable OnChange(Action<TenantsOptions, string> listener)
        {
            _listeners.Add(listener);
            return null;
        }

        public void Change(TenantsOptions options)
        {
            _currentValue = options;

            foreach (var listener in _listeners)
            {
                listener(options, string.Empty);
            }
        }
    }

}
