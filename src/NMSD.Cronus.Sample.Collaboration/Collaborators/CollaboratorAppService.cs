using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators
{
    public class CollaboratorAppService : AggregateRootApplicationService<Collaborator>, ICommandHandler,
        ICommandHandler<CreateNewCollaborator>,
        ICommandHandler<RenameCollaborator>
    {
        public void Handle(RenameCollaborator command)
        {
            UpdateAggregate(command.Id, ar => ar.Rename(command.FirstName, command.LastName));
        }

        public void Handle(CreateNewCollaborator command)
        {
            CreateAggregate(new Collaborator(command.Id, command.Email));
        }
    }

    //public class AggregateRootApplicationServiceG<T> where T : IAggregateRoot
    //{
    //    public dynamic EventStore { get; set; }

    //    protected T Load(IAggregateRootId id)
    //    {
    //        var events = EventStore.LoadEvents(id);
    //        return AggregateRootFactory.BuildFromHistory<T>(events);
    //    }


    //    protected void Save(T ar)
    //    {
    //        EventStore.Persist(ar.UncommittedEvents);
    //    }


    //}
    //public class CollaboratorAppServiceG : AggregateRootApplicationServiceG<Collaborator>, ICommandHandler,
    //   ICommandHandler<CreateNewCollaborator>,
    //   ICommandHandler<RenameCollaborator>
    //{
    //    public void Handle(RenameCollaborator command)
    //    {
    //        var ar = Load(command.Id);
    //        ar.Rename(command.FirstName, command.LastName);
    //        Save(ar);
    //    }

    //    public void Handle(CreateNewCollaborator command)
    //    {
    //        var collaborator = new Collaborator(command.Id, command.Email);
    //        Save(collaborator);
    //        event
    //    }
    //}

    //public class AggregateRepository
    //{
    
    //    public AggregateRepository()
    //    {

    //    }
    //    protected void UpdateAggregate(IAggregateRootId id, Action<Collaborator> updateAr)
    //    {
    //        var events = EventStore.LoadEvents(id);
    //        var aggregateRoot = AggregateRootFactory.BuildFromHistory<Collaborator>(events);
    //        updateAr(aggregateRoot);
    //        EventStore.Publish(aggregateRoot.UncommittedEvents);
    //    }

    //    protected void CreateAggregate(IAggregateRoot aggregateRoot)
    //    {
    //        EventStore.Publish(aggregateRoot.UncommittedEvents);
    //    }
    //}
}
