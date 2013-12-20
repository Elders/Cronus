using System.Collections.Generic;
namespace NSMD.Cronus.RabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            RabbitMqSessionFactory sf = new RabbitMqSessionFactory();
            var session = sf.OpenSession();
            var pipeline = new Pipeline("NewPipeline", session, Pipeline.PipelineType.Headers);
            pipeline.Declare();
            var endpoint = new Endpoint("NewEndpoint", session);
            endpoint.RoutingHeaders.Add("gtf", "1");
            endpoint.Declare();
            pipeline.AttachEndpoint(endpoint);
            endpoint.Open();

            //var msg = endpoint.BlockDequeue();

            endpoint.AcknowledgeAll();
            endpoint.Close();
            session.Close();
        }
    }
}
