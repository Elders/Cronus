using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra;
using Elders.Cronus.Persistence.Cassandra;

namespace Elders.Cronus.Sample.Cassandra
{
    class Program
    {
        static void Main(string[] args)
        {


            CassandraEventStore client = new CassandraEventStore();
            client.Connect("127.0.0.1");

            //client.CreateSchema();
            client.LoadData();
            client.Query();


            client.Close();

            Console.ReadLine();
        }
    }
}
