using System;
using System.Collections.Generic;
using Cassandra;
using Cassandra.Data.Linq;

using Elders.Cronus.EventSourcing;
using System.Text;

namespace Elders.Cronus.Persistence.Cassandra
{
    public class MyClass : ITable
    {
        public string ASD { get; set; }
        public void Create()
        {
            throw new NotImplementedException();
        }

        public Type GetEntityType()
        {
            throw new NotImplementedException();
        }

        public string GetQuotedTableName()
        {
            throw new NotImplementedException();
        }

        public Session GetSession()
        {
            throw new NotImplementedException();
        }

        public TableType GetTableType()
        {
            throw new NotImplementedException();
        }
    }
    public class CassandraEventStore : IEventStore
    {
        private Session _session;
        public Session Session { get { return _session; } }

        private Cluster cluster;

        public Cluster Cluster { get { return cluster; } }

        public void CreateSchema()
        {
            _session.Execute("CREATE KEYSPACE simplex WITH replication " +
          "= {'class':'SimpleStrategy', 'replication_factor':3};");

            _session.Execute(
      "CREATE TABLE simplex.songs (" +
            "id uuid PRIMARY KEY," +
            "title text," +
            "album text," +
            "artist text," +
            "tags set<text>," +
            "data blob" +
            ");");

            _session.Execute(
                  "CREATE TABLE simplex.playlists (" +
                        "id uuid," +
                        "title text," +
                        "album text, " +
                        "artist text," +
                        "song_id uuid," +
                        "PRIMARY KEY (id, title, album, artist)" +
                        ");");
        }

        public virtual void LoadData()
        {
            //var batch = Session.CreateBatch();
            //batch.Append()
            var table = _session.GetTable<MyClass>();
            table.Insert(new MyClass());


            _session.Execute(
                "INSERT INTO simplex.songs (id, title, album, artist, tags) " +
                "VALUES (" +
                "756716f7-2e54-4715-9f00-91dcbea6cf50," +
                "'La Petite Tonkinoise'," +
                "'Bye Bye Blackbird'," +
                "'Joséphine Baker'," +
                "{'jazz', '2013'})" +
                ";");
            _session.Execute(
                  "INSERT INTO simplex.playlists (id, song_id, title, album, artist) " +
                  "VALUES (" +
                      "2cc9ccb7-6221-4ccb-8387-f22b6a1b354d," +
                      "756716f7-2e54-4715-9f00-91dcbea6cf50," +
                      "'La Petite Tonkinoise'," +
                      "'Bye Bye Blackbird'," +
                      "'Joséphine Baker'" +
                      ");");


        }

        public void Query()
        {
            RowSet results = _session.Execute("SELECT * FROM playlists " +
        "WHERE id = 2cc9ccb7-6221-4ccb-8387-f22b6a1b354d;");

            Console.WriteLine(String.Format("{0, -30}\t{1, -20}\t{2, -20}\t{3, -30}",
        "title", "album", "artist", "tags"));
            Console.WriteLine("-------------------------------+-----------------------+--------------------+-------------------------------");
            foreach (Row row in results.GetRows())
            {
                Console.WriteLine(String.Format("{0, -30}\t{1, -20}\t{2, -20}\t{3}",
                        row.GetValue<String>("title"), row.GetValue<String>("album"),
                        row.GetValue<String>("artist"), row.GetValue<List<String>>("tags").ToString()));
            }
        }

        public void Connect(String node)
        {
            cluster = Cluster
                .Builder()
                .AddContactPoint(node)
                .Build();
            _session = cluster.Connect("simplex");
            Metadata metadata = cluster.Metadata;
            Console.WriteLine("Connected to cluster: " + metadata.ClusterName.ToString());
        }

        public void Close()
        {
            cluster.Shutdown();
        }





        public string BoundedContext
        {
            get { throw new System.NotImplementedException(); }
        }

        public System.Collections.Generic.IEnumerable<DomainModelling.IEvent> GetEventsFromStart(int batchPerQuery = 1)
        {
            throw new System.NotImplementedException();
        }

        public void UseStream(System.Func<DomainMessageCommit> getCommit, System.Func<IEventStream, DomainMessageCommit, bool> commitCondition, System.Action<System.Collections.Generic.List<DomainModelling.IEvent>> postCommit, System.Func<IEventStream, bool> closeStreamCondition, System.Action<DomainMessageCommit> onPersistError)
        {
            throw new System.NotImplementedException();
        }
    }
}
