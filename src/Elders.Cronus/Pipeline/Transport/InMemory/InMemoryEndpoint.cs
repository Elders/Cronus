using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Pipeline.Transport.InMemory
{
    public class InMemoryEndpoint : IEndpoint
    {
        private readonly InMemoryPipelineTransport transport;
        public InMemoryEndpoint(InMemoryPipelineTransport transport, string name, ICollection<string> watchMessageTypes)
        {
            this.transport = transport;
            Name = name;
            WatchMessageTypes = watchMessageTypes;
        }

        public string Name { get; set; }
        public ICollection<string> WatchMessageTypes { get; set; }

        private bool Started = false;
        private bool ShouldWork = false;
        private Task ActiveTask;
        private Action<CronusMessage> onMessage = null;

        public void OnMessage(Action<CronusMessage> action)
        {
            if (Started)
            {
                throw new Exception("Cannot assign 'onMessage' handler when endpoint was started already");
            }

            onMessage = action;
        }

        public void Start()
        {
            if (Started)
            {
                throw new Exception("Cannot start endpoint because it's already started");
            }

            if (onMessage == null)
            {
                throw new Exception("Cannot start endpoint because onMessage handler was not specified yet!");
            }

            Started = true;

            ShouldWork = true;
            ActiveTask = new Task(() =>
            {
                while (ShouldWork)
                {
                    try
                    {
                        CronusMessage msg;
                        if (!transport.BlockDequeue(this, 1000, out msg))
                        {
                            Thread.Sleep(300);
                            continue;
                        }

                        onMessage(msg);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("InMemoryEndpoint error: {0}", ex.Message);
                    }
                }
            });
            ActiveTask.Start();
        }

        public void Stop()
        {
            if (!Started)
            {
                throw new Exception("Cannot stop endpoint because it was not started!");
            }

            Started = false;
            ShouldWork = false;
            ActiveTask.Wait();
            ActiveTask = null;
        }

        public override bool Equals(System.Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(InMemoryEndpoint)) return false;
            return Equals((InMemoryEndpoint)obj);
        }

        public virtual bool Equals(IEndpoint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 17 ^ Name.GetHashCode();
            }
        }

        public static bool operator ==(InMemoryEndpoint left, InMemoryEndpoint right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left))
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(InMemoryEndpoint a, InMemoryEndpoint b)
        {
            return !(a == b);
        }
    }
}