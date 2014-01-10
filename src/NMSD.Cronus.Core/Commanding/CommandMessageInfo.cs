using System;
using NMSD.Cronus.Core.Messaging;

namespace NMSD.Cronus.Core.Commanding
{
    public static class CommandMessageInfo
    {
        public static string ToString(this IMessage message, string info, params object[] args)
        {
            var bcNamespace = MessageInfo.GetBoundedContextNamespace(message.GetType());
            var messageInfo = String.Format(info, args);
            return String.Format("[{0}] {1}.", bcNamespace, messageInfo);
        }
    }
}
