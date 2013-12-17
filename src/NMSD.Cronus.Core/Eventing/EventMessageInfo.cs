using System;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Messaging;

namespace NMSD.Cronus.Core.Eventing
{
    public static class EventMessageInfo
    {
        public static string ToString(this IMessage message, string info, params object[] args)
        {
            var bcNamespace = MessagingHelper.GetBoundedContextNamespace(message.GetType());
            var messageInfo = String.Format(info, args);
            return String.Format("[{0}] {1}.", bcNamespace, messageInfo);
        }
    }
}
