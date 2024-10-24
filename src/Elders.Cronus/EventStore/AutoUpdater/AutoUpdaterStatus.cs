using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater
{
    [DataContract(Name = "b19d21ea-d25c-4876-aa24-159a440ad631")]
    public record class AutoUpdaterStatus
    {
        AutoUpdaterStatus() { }

        AutoUpdaterStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentNullException("status");

            Status = status;
        }

        [DataMember(Order = 1)]
        public string Status { get; private set; }

        /// <summary>
        /// This is the first time the AR is being initialized so nothing to update to
        /// </summary>
        public static AutoUpdaterStatus None = new AutoUpdaterStatus("none");

        /// <summary>
        /// Update is currently in process/>
        /// </summary>
        public static AutoUpdaterStatus Triggered = new AutoUpdaterStatus("triggered");

        /// <summary>
        /// Update is finished/>
        /// </summary>
        public static AutoUpdaterStatus Finished = new AutoUpdaterStatus("finished");

        /// <summary>
        /// Update is canceled and thus failed/>
        /// </summary>
        public static AutoUpdaterStatus Failed = new AutoUpdaterStatus("failed");
    }
}
