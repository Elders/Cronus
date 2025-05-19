using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elders.Cronus.EventStore.Index;

public class EventLookupInByteArray
{
    private readonly Dictionary<string, byte[]> table;

    public EventLookupInByteArray(TypeContainer<IEvent> events, TypeContainer<IPublicEvent> publicEvents)
    {
        table = BuildTable(events, publicEvents);
    }

    private static Dictionary<string, byte[]> BuildTable(TypeContainer<IEvent> events, TypeContainer<IPublicEvent> publicEvents)
    {
        Dictionary<string, byte[]> messageToByteArray = new Dictionary<string, byte[]>();

        Type entityEvent = typeof(EntityEvent);
        foreach (Type eventType in events.Items)
        {
            if (eventType == entityEvent)
                continue;

            string contractId = eventType.GetContractId();
            byte[] bytes = Encoding.UTF8.GetBytes(contractId);
            messageToByteArray.TryAdd(contractId, bytes);
        }
        foreach (Type eventType in publicEvents.Items)
        {
            string contractId = eventType.GetContractId();
            byte[] bytes = Encoding.UTF8.GetBytes(contractId);
            messageToByteArray.TryAdd(contractId, bytes);
        }

        return messageToByteArray;
    }

    public string FindEventId(ReadOnlySpan<byte> data)
    {
        ReadOnlySpan<byte> contract = data.Slice(10, 36);
        foreach (var item in table)
        {
            if (item.Value.AsSpan().SequenceEqual(contract))
            {
                return item.Key;
            }
        }

        return string.Empty;
    }

    public bool HasEventId(ReadOnlySpan<byte> data, ReadOnlySpan<byte> eventContract)
    {
        ReadOnlySpan<byte> contract = data.Slice(10, 36);
        return contract.SequenceEqual(eventContract);
    }
}
