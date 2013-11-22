using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Cronus.Core.Eventing;
using Protoreg;

namespace NMSD.Cronus.Core.EventStoreEngine
{
    [DataContract(Namespace = "dlfkgsdlfghsldfhglsdfkhg", Name = "987a7bed-7689-4c08-b610-9a802d306215")]
    public class Wraper
    {
        [DataMember(Order = 1)]
        public List<object> Events { get; set; }
        /// <summary>
        /// Initializes a new instance of the wraper class.
        /// </summary>
        public Wraper(List<object> events)
        {
            Events = events;
        }
        public Wraper()
        {

        }
    }
    public class InMemoryEventStore
    {

        private readonly ProtoregSerializer serializer;
        public InMemoryEventStore(ProtoregSerializer serializer)
        {
            this.serializer = serializer;
        }
        public void Persist(List<IEvent> events)
        {
            byte[] blob;
            using (var str = new MemoryStream())
            {
                serializer.Serialize(str, new Wraper(events.Cast<object>().ToList()));
                blob = str.ToArray();
            }

            DataTable dt = MakeTable();

            var row = dt.NewRow();
            row[1] = blob;
            row[2] = 1;
            row[3] = DateTime.UtcNow;
            dt.Rows.Add(row);

            var connection = new SqlConnection("Server=.;Database=CronusES;User Id=sa;Password=sa;");
            connection.Open();
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = "dbo.BoundedContext";

                try
                {
                    bulkCopy.WriteToServer(dt, DataRowState.Added);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            connection.Close();
        }

        private static DataTable MakeTable()
        {
            DataTable uncommittedEvents = new DataTable("BoundedContext");

            DataColumn revision = new DataColumn();
            revision.DataType = typeof(int);
            revision.ColumnName = "Revision";
            revision.AutoIncrement = true;
            revision.Unique = true;
            uncommittedEvents.Columns.Add(revision);

            DataColumn events = new DataColumn();
            events.DataType = typeof(byte[]);
            events.ColumnName = "Events";
            uncommittedEvents.Columns.Add(events);

            DataColumn eventsCount = new DataColumn();
            eventsCount.DataType = typeof(uint);
            eventsCount.ColumnName = "EventsCount";
            uncommittedEvents.Columns.Add(eventsCount);

            DataColumn timestamp = new DataColumn();
            timestamp.DataType = typeof(DateTime);
            timestamp.ColumnName = "Timestamp";
            uncommittedEvents.Columns.Add(timestamp);

            DataColumn[] keys = new DataColumn[1];
            keys[0] = revision;
            uncommittedEvents.PrimaryKey = keys;

            return uncommittedEvents;
        }
    }

    public static class MeasureExecutionTime
    {
        public static string Start(Action action)
        {
            string result = string.Empty;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            action();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            return result;
        }

        public static string Start(Action action, int repeat)
        {
            string result = string.Empty;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i <= repeat; i++)
            {
                action();
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            return result;
        }

        public static string Start(Action<int> action, int repeat)
        {
            string result = string.Empty;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i <= repeat; i++)
            {
                action(i);
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            return result;
        }
    }
}
