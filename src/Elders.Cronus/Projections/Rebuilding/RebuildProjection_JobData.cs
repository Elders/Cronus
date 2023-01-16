using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.EventStore.Index;

namespace Elders.Cronus.Projections.Rebuilding
{
    public class RebuildProjection_JobData : IJobData
    {
        public RebuildProjection_JobData()
        {
            IsCompleted = false;
            IsCanceled = false;
            EventTypePaging = new List<EventPaging>();
            Timestamp = DateTimeOffset.UtcNow;
            DueDate = DateTimeOffset.MaxValue;
        }

        public bool IsCompleted { get; set; }

        public bool IsCanceled { get; set; }

        public List<EventPaging> EventTypePaging { get; set; }

        public ProjectionVersion Version { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public DateTimeOffset DueDate { get; set; }

        public bool MarkEventTypeProgress(EventPaging progress)
        {
            EventPaging existing = EventTypePaging.Where(et => et.Type.Equals(progress.Type, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (existing is null)
            {
                EventTypePaging.Add(progress);
                return true;
            }
            else
            {
                if (existing.PaginationToken.Equals(progress.PaginationToken) == false)
                {
                    existing.PaginationToken = progress.PaginationToken;
                    existing.ProcessedCount = progress.ProcessedCount;
                    existing.TotalCount = progress.TotalCount;

                    return true;
                }
            }

            return false;
        }

        public void Init(EventPaging progress)
        {
            EventPaging existing = EventTypePaging.Where(et => et.Type.Equals(progress.Type, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (existing is null)
            {
                EventTypePaging.Add(progress);
            }
        }

        public partial class EventPaging
        {
            public EventPaging(string eventTypeId, string paginationToken, DateTimeOffset? after, DateTimeOffset? before, ulong processedCount, ulong totalCount)
            {
                Type = eventTypeId;
                PaginationToken = paginationToken;
                After = after;
                Before = before;
                ProcessedCount = processedCount;
                TotalCount = totalCount;
            }

            public string Type { get; set; }

            public string PaginationToken { get; set; }

            public DateTimeOffset? After { get; set; }
            public DateTimeOffset? Before { get; set; }

            public ulong ProcessedCount { get; set; }

            public ulong TotalCount { get; set; }
        }
    }
}
