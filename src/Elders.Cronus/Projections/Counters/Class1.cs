//using System;
//using System.Collections.Generic;
//using System.Runtime.Serialization;
//using System.Text;

//namespace Elders.Cronus.Projections.Counters
//{
//    [DataContract(Name = ContractId)]
//    public class EventCounters : ProjectionDefinition<ProjectionVersionsHandlerState, ProjectionVersionManagerId>, IAmNotSnapshotable, ISystemProjection, INonVersionableProjection,
//       IEventHandler<ProjectionVersionRequested>,
//       IEventHandler<NewProjectionVersionIsNowLive>,
//       IEventHandler<ProjectionVersionRequestCanceled>,
//       IEventHandler<ProjectionVersionRequestTimedout>
//    {
//        public const string ContractId = "f1469a8e-9fc8-47f5-b057-d5394ed33b4c";
//    }
