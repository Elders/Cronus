using System;
using NMSD.Cronus.Messaging;

namespace NMSD.Cronus.Eventing
{
    public static class EventMessageInfo
    {
        public static string ToString(this IMessage message, string info, params object[] args)
        {
            var bcNamespace = message.GetType().GetBoundedContext().BoundedContextNamespace;
            var messageInfo = String.Format(info, args);
            return String.Format("[{0}] {1}.", bcNamespace, messageInfo);
        }
    }
}
