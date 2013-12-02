using System.Runtime.Serialization;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Commands
{
    [BoundedContext("NMSD.Cronus.Sample.Collaboration")]
    public interface ICollaborationCommand : ICommand
    {

    }
}
