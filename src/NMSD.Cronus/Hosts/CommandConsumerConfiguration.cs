using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using NMSD.Cronus.UnitOfWork;
namespace NMSD.Cronus.Hosts
{
    public class CommandConsumerConfiguration
    {
        public Assembly AggregateStatesAssembly { get; private set; }

        public Assembly CommandHandlersAssembly { get; private set; }

        public Assembly CommandsAssembly { get; private set; }

        public Assembly EventsAssembly { get; private set; }

        public IUnitOfWorkFactory UnitOfWorkFacotry { get; private set; }

        public string EventStoreConnectionString { get; private set; }

        public void SetEventStoreConnectionString(string eventStoreConnectionString)
        {
            EventStoreConnectionString = eventStoreConnectionString;
        }

        public void SetAggregateStatesAssembly(Assembly aggregateStatesAssembly)
        {
            AggregateStatesAssembly = aggregateStatesAssembly;
        }

        public void SetCommandHandlersAssembly(Assembly commandHandlersAssembly)
        {
            CommandHandlersAssembly = commandHandlersAssembly;
        }

        public void SetCommandsAssembly(Assembly commandsAssembly)
        {
            CommandsAssembly = commandsAssembly;
        }

        public void SetEventsAssembly(Assembly eventsAssembly)
        {
            EventsAssembly = eventsAssembly;
        }

        public void SetUnitOfWorkFacotry(IUnitOfWorkFactory unitOfWorkFacotry)
        {
            UnitOfWorkFacotry = unitOfWorkFacotry;
        }

    }
}
