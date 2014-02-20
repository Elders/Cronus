using System.Collections.Generic;
using System.Reflection;
using NMSD.Cronus.UnitOfWork;

namespace NMSD.Cronus.Hosts
{
    public class EventHandlersConfiguration
    {
        public Assembly EventHandlersAssembly { get; private set; }

        public HashSet<Assembly> EventsAssemblies { get; private set; }

        public IUnitOfWorkFactory UnitOfWorkFacotry { get; private set; }

        public EventHandlersConfiguration()
        {
            EventsAssemblies = new HashSet<Assembly>();
        }

        public void SetEventHandlersAssembly(Assembly commandHandlersAssembly)
        {
            EventHandlersAssembly = commandHandlersAssembly;
        }

        public void RegisterEventsAssembly(Assembly eventsAssembly)
        {
            EventsAssemblies.Add(eventsAssembly);
        }

        public void SetUnitOfWorkFacotry(IUnitOfWorkFactory unitOfWorkFacotry)
        {
            UnitOfWorkFacotry = unitOfWorkFacotry;
        }

    }
}