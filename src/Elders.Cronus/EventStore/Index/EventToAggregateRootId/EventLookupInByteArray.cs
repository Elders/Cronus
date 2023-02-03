using System;
using System.Collections.Concurrent;
using System.Text;

namespace Elders.Cronus.EventStore.Index
{
    public class EventLookupInByteArray
    {
        ConcurrentDictionary<string, byte[]> messageToByteArray = new ConcurrentDictionary<string, byte[]>();

        public EventLookupInByteArray(TypeContainer<IEvent> events, TypeContainer<IPublicEvent> publicEvents)
        {
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
        }

        public string FindEvent(byte[] source)
        {
            foreach (var item in messageToByteArray)
            {
                if (FindSequence(source, 0, item.Value) > -1)
                    return item.Key;
            }

            return string.Empty;
        }

        /// <summary>Looks for the next occurrence of a sequence in a byte array.</summary>
        /// <param name="array">Array that will be scanned</param>
        /// <param name="start">Index in the array at which scanning will begin</param>
        /// <param name="sequence">Sequence the array will be scanned for</param>
        /// <returns>
        /// The index of the next occurrence of the sequence of -1 if not found
        /// </returns>
        private static int FindSequence(byte[] array, int start, byte[] sequence)
        {
            int end = array.Length - sequence.Length; // past here no match is possible
            byte firstByte = sequence[0]; // cached to tell compiler there's no aliasing

            while (start <= end)
            {
                // scan for first byte only. compiler-friendly.
                if (array[start] == firstByte)
                {
                    // scan for rest of sequence
                    for (int offset = 1; ; ++offset)
                    {
                        if (offset == sequence.Length)
                        {
                            return start; // full sequence matched?
                        }
                        else if (array[start + offset] != sequence[offset])
                        {
                            break;
                        }
                    }
                }
                ++start;
            }

            return -1; // end of array reached without match
        }

    }
}
