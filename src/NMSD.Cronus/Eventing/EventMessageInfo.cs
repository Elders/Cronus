using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;

namespace NMSD.Cronus.Eventing
{
    public static class EventMessageInfo
    {
        public static string ToString(this IMessage message, string info, params object[] args)
        {
            var bcNamespace = MessageInfo.GetBoundedContextNamespace(message.GetType());
            var messageInfo = String.Format(info, args);
            return String.Format("[{0}] {1}.", bcNamespace, messageInfo);
        }
    }
}
