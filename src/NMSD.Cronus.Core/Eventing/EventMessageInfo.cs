using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.DomainModelling;
using NMSD.Cronus.Core.Messaging;

namespace NMSD.Cronus.Core.Eventing
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
