using System;

namespace Elders.Cronus.Pipeline
{
    public sealed class MessageThreshold
    {
        public MessageThreshold() : this(1, 30) { }

        /// <summary>
        /// If the size is > 1 and the delay is 0 could be dangerous. Use only in special cases and you should be familiar with the PipelineConsumerWork code.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="delay"></param>
        public MessageThreshold(uint size, uint delay)
        {
            if (size == 0) throw new ArgumentException("The size cannot be 0", "size");

            if (size != 1) throw new Exception("Ask Simo, hahahahaha");
            this.Size = size;
            this.Delay = delay;
        }

        public uint Size { get; private set; }
        public uint Delay { get; private set; }
    }
}