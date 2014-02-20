using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.EventSourcing;

namespace NMSD.Cronus.Player
{
    public class EventPlayer : IEventPlayer
    {
        private readonly IEventStore eventStore;
        public EventPlayer(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public void ReplayEvents()
        {
            foreach (var evnt in eventStore.GetEventsFromStart("IdentityAndAccess"))
            {
                Console.WriteLine(evnt.ToString());
            }
        }
    }
}
