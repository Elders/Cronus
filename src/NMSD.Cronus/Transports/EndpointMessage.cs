using System.Collections.Generic;

namespace NMSD.Cronus.Transports
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