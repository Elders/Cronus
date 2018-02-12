using System;
using System.Collections.Generic;

namespace Elders.Cronus.MessageProcessing
{
    public interface ISubscriber
    {
        string Id { get; }

        /// <summary>
        /// Gets the message types which the subscriber can process.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetInvolvedMessageTypes();

        void Process(CronusMessage message);
    }
}
