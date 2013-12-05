using System;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Messaging;

namespace NMSD.Cronus.Core.Commanding
{
    public static class CommandMessageInfo
    {
        public static string ToString(this ICommand command, string info, params object[] args)
        {
            var bcNamespace = MessagingHelper.GetBoundedContextNamespace(command.GetType());
            var commandMessage = String.Format(info, args);
            return String.Format("[{0}] {1}.", bcNamespace, commandMessage);
        }
    }
}
