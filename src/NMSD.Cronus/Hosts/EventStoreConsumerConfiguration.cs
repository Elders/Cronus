using System;
using System.Reflection;
using NMSD.Cronus.UnitOfWork;

namespace NMSD.Cronus.Hosts
{
    public class EventStoreConsumerConfiguration
    {
        public Assembly AggregateStatesAssembly { get; private set; }

        public Type AssemblyContainingEventsByEventType { get; private set; }

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


        public void SetEventsAssembly(Type assemblyContainingEventsByEventType)
        {
            AssemblyContainingEventsByEventType = assemblyContainingEventsByEventType;
        }

        public void SetUnitOfWorkFacotry(IUnitOfWorkFactory unitOfWorkFacotry)
        {
            UnitOfWorkFacotry = unitOfWorkFacotry;
        }

    }
}