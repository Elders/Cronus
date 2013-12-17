using System;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Messaging;

namespace NMSD.Cronus.Core.Messaging
{
    public static class MessageInfo
    {
        public static string ToString(this IMessage message, string info, params object[] args)
        {
            var bcNamespace = MessagingHelper.GetBoundedContextNamespace(message.GetType());
            var messageInfo = String.Format(info, args);
            return String.Format("[{0}] {1}.", bcNamespace, messageInfo);
        }
    }
}
