using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NSMD.Cronus.RabbitMQ
{
    public class EndpointMessage
    {

        public EndpointMessage(byte[] body, IDictionary<string, object> headers)
        {
            Headers = headers;
            Body = body;
        }

        public EndpointMessage(byte[] body)
        {
            Body = body;
            Headers = new Dictionary<string, object>();
        }
        public IDictionary<string, object> Headers { get; set; }
        public byte[] Body { get; set; }
    }
}
