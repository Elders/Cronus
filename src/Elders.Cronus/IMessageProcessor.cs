using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus
{
    public interface IMessageProcessor<TMessage> where TMessage : IMessage
    {
        ISafeBatchResult<TransportMessage> Handle(List<TransportMessage> messages);
        IEnumerable<Type> GetRegisteredHandlers();
    }



    //public interface IProjectionStore
    //{
    //    void Persist(ProjectionCommit commit);
    //}

    //public class ProjectionEventStreamProcessor : IMessageProcessor<IEvent>
    //{
    //    IProjectionStore projectionStore;

    //    public ProjectionEventStreamProcessor(IProjectionStore projectionStore)
    //    {

    //    }

    //    public IEnumerable<Type> GetRegisteredHandlers()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public ISafeBatchResult<TransportMessage> Handle(List<TransportMessage> messages)
    //    {
    //        object proj = null;
    //        ProjectionCommit projCommit = null;
    //        projectionStore.Persist(projCommit);
    //    }
    //}

    //public class ProjectionId
    //{

    //}

    //public class ProjectionCommit
    //{
    //    public ProjectionCommit(ProjectionId id, List<IEvent> events)
    //    {

    //    }
    //}
}
