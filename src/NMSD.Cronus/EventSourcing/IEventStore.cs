using System;
using System.Collections.Generic;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Eventing;

namespace NMSD.Cronus.EventSourcing
{
    public interface IEventStore
    {
        /// <summary>
        /// Opens an empty stream.
        /// </summary>
        /// <param name="getCommit">How to get a single <see cref="DomainMessageCommit"/> </param>
        /// <param name="commitCondition">When to commit the stream. IEventStream param holds the current state of the stream. DomainMessageCommit param holds the result of getCommit.</param>
        /// <param name="postCommit">What to do after a successful commit.</param>
        /// <param name="closeStreamCondition">When to close the stream.</param>
        void UseStream(Func<DomainMessageCommit> getCommit, Func<IEventStream, DomainMessageCommit, bool> commitCondition, Action<List<IEvent>> postCommit, Func<IEventStream, bool> closeStreamCondition, Action<DomainMessageCommit> onPersistError);

        IEnumerable<IEvent> GetEventsFromStart(string boundedContext, int batchPerQuery = 1);
    }

    public interface IEventStream
    {
        List<IEvent> Events { get; }
        List<IAggregateRootState> Snapshots { get; }
    }
}