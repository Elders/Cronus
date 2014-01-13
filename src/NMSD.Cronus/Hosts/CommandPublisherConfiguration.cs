using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.Hosts
{
    public class CommandPublisherConfiguration
    {
        public CommandPublisherConfiguration()
        {
            this.CommandsAssemblies = new HashSet<Assembly>();
        }

        public HashSet<Assembly> CommandsAssemblies { get; private set; }

        public void RegisterCommandsAssembly(Assembly assembly)
        {
            CommandsAssemblies.Add(assembly);
        }
        public void RegisterCommandsAssembly<T>()
        {
            CommandsAssemblies.Add(typeof(T).Assembly);
        }

    }
}
