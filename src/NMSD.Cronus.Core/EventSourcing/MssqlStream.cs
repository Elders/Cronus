using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Messaging;
using NMSD.Protoreg;
using System.Text;
using System.Globalization;
using NMSD.Cronus.Core.DomainModelling;

namespace NMSD.Cronus.Core.EventSourcing
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
