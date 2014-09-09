//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.Serialization;
//using Elders.Cronus.DomainModeling;

//namespace Elders.Cronus.Pipeline.Strategy
//{
//    public class ProjectionEndpointPerHandler : EndpointNameConventionWithCorrespondingErrorEndpoint
//    {
//        private IPipelineNameConvention pipelineNameConvention;

//        public ProjectionEndpointPerHandler(IPipelineNameConvention pipelineNameConvention)
//        {
//            this.pipelineNameConvention = pipelineNameConvention;
//        }
//        public override IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes)
//        {
//            foreach (var handlerType in handlerTypes)
//            {
//                var boundedContext = handlerType.GetBoundedContext();

//                if (boundedContext == null)
//                    throw new Exception(String.Format(@"The assembly containing message type '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", handlerType.FullName));

//                string endpointName = String.Format("{0}.Projections.{1}", boundedContext.BoundedContextNamespace, handlerType.Name);

//                var eventTypes = handlerType.GetMethods().Where(x => x.Name == "Handle").SelectMany(x => x.GetParameters().Select(y => y.ParameterType));
//                var eventIds = eventTypes.Select(x => (x.GetCustomAttribute(typeof(DataContractAttribute), false) as DataContractAttribute).Name);
//                var acceptanceHeaders = new Dictionary<string, object>();
//                foreach (var id in eventIds)
//                {
//                    acceptanceHeaders[id] = String.Empty;
//                }
//                EndpointDefinition endpointDefinition = new EndpointDefinition(pipelineNameConvention.GetPipelineName(boundedContext), endpointName, acceptanceHeaders);
//                yield return endpointDefinition;
//            }
//        }
//    }
//}