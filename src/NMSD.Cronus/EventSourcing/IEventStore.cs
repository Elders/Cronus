using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Eventing;

namespace NMSD.Cronus.EventSourcing
{
    public interface IEventStore
    {
        IEventStream OpenStream();

        void Commit(IEventStream stream);
    }
    public interface IEventStream : IDisposable
    {
        List<IEvent> Events { get; }
        List<IAggregateRootState> Snapshots { get; }
        void Close();
    }
    public static class IStreamExtensions
    {
        public static void Clear(this IEventStream str)
        {
            str.Events.Clear();
            str.Snapshots.Clear();
        }
    }
}