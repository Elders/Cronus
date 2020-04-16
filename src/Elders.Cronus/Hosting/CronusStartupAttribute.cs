using System;

namespace Elders.Cronus
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CronusStartupAttribute : Attribute
    {
        public CronusStartupAttribute() : this(Bootstraps.Runtime) { }

        public CronusStartupAttribute(Bootstraps bootstraps)
        {
            Bootstraps = bootstraps;
        }

        public Bootstraps Bootstraps { get; }
    }

    public enum Bootstraps
    {
        /// <summary>
        /// Bootstraps the environment and prapare it for Cronus
        /// </summary>
        Environment = 0,

        /// <summary>
        /// Bootstraps external resources such as database or message broker services
        /// </summary>
        ExternalResource = 10,

        /// <summary>
        /// Bootstraps configuration settings and options for the application
        /// </summary>
        Configuration = 20,

        Aggregates = 30,
        Ports = 40,
        Sagas = 50,
        EventStoreIndices = 55,
        Projections = 60,
        Gateways = 70,

        /// <summary>
        /// Bootstraps anything else. It is executed last and is the default
        /// </summary>
        Runtime = 1000,
    }
}
