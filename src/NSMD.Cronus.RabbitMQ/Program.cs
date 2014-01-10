using System.Collections.Generic;
using NMSD.Cronus.Sample.Collaboration.Projections;
using System.Linq;
using System;
using NMSD.Cronus.Core.Transports.Conventions;
namespace NMSD.Cronus.RabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            Type handler = typeof(CollaboratorProjection);
          //  var conventionPerHandler = new EndpointPerEventHandler();
           // var definition = conventionPerHandler.GetEndpointDefinitions().First();

           // var conventionPerBoundedContext = new EventHandlersPerBoundedContext();
           // var def2 = conventionPerBoundedContext.GetEndpointDefinitions();
            //RabbitMqSessionFactory sf = new RabbitMqSessionFactory();
            //var session = sf.OpenSession();
            //var pipeline = new RabbitMqPipeline("NewPipeline", session, RabbitMqPipeline.PipelineType.Headers);
            //pipeline.Declare();
            //var endpoint = new RabbitMqEndpoint("NewEndpoint", session);
            //endpoint.RoutingHeaders.Add("gtf", "1");
            //endpoint.Declare();
            //pipeline.AttachEndpoint(endpoint);
            //endpoint.Open();

            ////var msg = endpoint.BlockDequeue();

            //endpoint.AcknowledgeAll();
            //endpoint.Close();
            //session.Close();
        }
    }
}
