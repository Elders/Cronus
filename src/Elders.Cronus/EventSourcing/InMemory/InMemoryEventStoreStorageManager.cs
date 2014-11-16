using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.DomainModeling;
using System.Collections.Concurrent;

namespace Elders.Cronus.EventSourcing.InMemory
{
    public class InMemoryEventStoreStorageManager : IEventStoreStorageManager
    {
        public void CreateStorage()
        {
            this.CreateEventsStorage();
            this.CreateSnapshotsStorage();
        }

        public void CreateEventsStorage()
        {

        }

        public void CreateSnapshotsStorage()
        {

        }
    }
}