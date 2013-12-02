using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators
{
    [BoundedContext("NMSD.Cronus.Sample.Collaboration")]
    public interface ICollaborationCommandHandler : ICommandHandler
    {

    }
}