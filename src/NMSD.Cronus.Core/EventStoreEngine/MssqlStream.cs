using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NMSD.Cronus.Core.Eventing;
using Cronus.Core.EventStore;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Snapshotting;
using NMSD.Protoreg;
using System.Text;
using System.Globalization;

namespace NMSD.Cronus.Core.EventStoreEngine
{
    public class MssqlStream : IEventStream
    {
        public MssqlStream(SqlConnection connection)
        {
            Connection = connection;
            Events = new List<IEvent>();
            Snapshots = new List<IAggregateRootState>();
        }

        public SqlConnection Connection { get; private set; }

        public List<IEvent> Events { get; private set; }

        public List<IAggregateRootState> Snapshots { get; private set; }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
            }
        }

    }
}
