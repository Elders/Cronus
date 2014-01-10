using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.Snapshotting
{
    public interface IPersistEventStream
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