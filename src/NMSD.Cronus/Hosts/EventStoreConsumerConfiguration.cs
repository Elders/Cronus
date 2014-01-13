using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.UnitOfWork;

namespace NMSD.Cronus.Hosts
{
    public class EventStoreConsumerConfiguration
    {
        public Assembly AggregateStatesAssembly { get; private set; }

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
