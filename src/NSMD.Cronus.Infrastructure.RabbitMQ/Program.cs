using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace NSMD.Cronus.RabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    public abstract class BasicConsumer
    {
        private readonly Endpoint endpoint;
        public BasicConsumer(Endpoint endpoint)
        {
            this.endpoint = endpoint;
        }

        protected abstract void Consume(object message)
        {

        }

        public void Start()
        {
            endpoint.
        }
    }

}
