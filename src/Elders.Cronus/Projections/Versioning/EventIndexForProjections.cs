using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = IndexByEventTypeSubscriber.ContractId)]
    public class IndexByEventTypeSubscriber : ISubscriber
    {
        public const string ContractId = "c8091ae7-a75a-4d66-a66b-de740f6bf9fd";

        private readonly TypeContainer<IEvent> allEventTypesInTheSystem;
        private readonly IServiceProvider ioc;
        private readonly Func<IServiceScope, IIndexStore> indexProvider;

        public string Id { get { return nameof(IndexByEventTypeSubscriber); } }

        public IndexByEventTypeSubscriber(TypeContainer<IEvent> allEventTypesInTheSystem, IServiceProvider ioc, Func<IServiceScope, IIndexStore> indexProvider)
        {
            this.allEventTypesInTheSystem = allEventTypesInTheSystem;
            this.ioc = ioc;
            this.indexProvider = indexProvider;
        }

        public IEnumerable<Type> GetInvolvedMessageTypes()
        {
            return allEventTypesInTheSystem.Items;
        }

        public void Process(CronusMessage message)
        {
            using (IServiceScope scope = ioc.CreateScope())
            {
                var cronusContextFactory = scope.ServiceProvider.GetRequiredService<CronusContextFactory>();
                CronusContext cronusContext = cronusContextFactory.GetContext(message, scope.ServiceProvider);

                var index = indexProvider(scope);
                var indexRecord = new List<IndexRecord>();
                var @event = message.Payload as IEvent;

                string eventTypeId = @event.Unwrap().GetType().GetContractId();
                indexRecord.Add(new IndexRecord(eventTypeId, Encoding.UTF8.GetBytes(message.GetRootId())));
                index.Apend(indexRecord);
            }

        }
    }
}
