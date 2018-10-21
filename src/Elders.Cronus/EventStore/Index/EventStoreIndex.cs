using Elders.Cronus.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.EventStore.Index
{
    public class EventStoreIndex
    {
        static ILog log = LogProvider.GetLogger(typeof(EventStoreIndex));

        public const string StateId = "55f9e248-7bb3-4288-8db8-ba9620c67228";

        private object rebuildSync = new object();
        private bool isBuilding = false;

        private readonly IIndexStatusStore indexStatus;
        private readonly IIndexStore indexStore;

        public EventStoreIndex(IIndexStatusStore indexStatus, IIndexStore indexStore)
        {
            this.indexStatus = indexStatus;
            this.indexStore = indexStore;
        }

        public bool Rebuild(Func<IEnumerable<IndexRecord>> indexRecords)
        {
            if (Prepare())
            {
                indexStore.Apend(indexRecords());
                Complete();
            }

            return false;
        }

        bool Prepare()
        {
            if (CanBuild())
            {
                lock (rebuildSync)
                {
                    if (CanBuild())
                    {
                        indexStatus.Save(StateId, IndexStatus.Building);
                        isBuilding = true;
                    }
                    else
                    {
                        log.Debug(() => "Index is currently built by someone else");
                        return false;
                    }
                }
            }
            else
            {
                log.Debug(() => "Index is currently built by someone");
                return false;
            }

            return true;
        }

        bool CanBuild()
        {
            return isBuilding == false || indexStatus.Get(StateId).IsNotBuilding();
        }

        void Complete()
        {
            indexStatus.Save(StateId, IndexStatus.Present);
            isBuilding = false;
        }

        public IEnumerable<IndexRecord> EnumerateRecords(string dataId)
        {
            // TODO: index exists?
            return indexStore.Get(dataId);
        }

        public IndexStatus Status { get { return indexStatus.Get(StateId); } }
    }
}
