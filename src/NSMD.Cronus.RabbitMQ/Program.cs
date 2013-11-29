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
            var simo = new Plumber();
            var pipe = simo.GetPipeline("schupena_traba");
            // var chuk = simo.GetEndpoint("chuk", "");
            // var tesla = simo.GetEndpoint("tesla", "");
            //pipe.AttachEndpoint(chuk);
            // pipe.AttachEndpoint(tesla);
            pipe.KickIn(new byte[] { 101, 123 }, "1");
            pipe.Dispose();
            // chuk.Dispose();


        }
    }

    public abstract class BasicConsumer
    {
        private readonly Endpoint endpoint;
        public BasicConsumer(Endpoint endpoint)
        {
            this.endpoint = endpoint;
        }

        protected abstract void Consume(object message);

        public void Start()
        {
            // endpoint.
        }
    }

}
