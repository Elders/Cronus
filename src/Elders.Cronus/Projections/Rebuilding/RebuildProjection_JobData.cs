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
            EventTypePaging = new List<EventPaging>();
            Timestamp = DateTimeOffset.UtcNow;
            DueDate = DateTimeOffset.MaxValue;
        }

        public bool IsCompleted { get; set; }

        public List<EventPaging> EventTypePaging { get; set; }

        public ProjectionVersion Version { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public DateTimeOffset DueDate { get; set; }

        public void MarkEventTypeProgress(EventPaging progress)
        {
            EventPaging existing = EventTypePaging.Where(et => et.Type.Equals(progress.Type, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (existing is null)
            {
                EventTypePaging.Add(progress);
            }
            else
            {
                existing.PaginationToken = progress.PaginationToken;
                existing.ProcessedCount = progress.ProcessedCount;
                existing.TotalCount = progress.TotalCount;
            }
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
            public EventPaging(string eventTypeId, string paginationToken, long processedCount, long totalCount)
            {
                Type = eventTypeId;
                PaginationToken = paginationToken;
                ProcessedCount = processedCount;
                TotalCount = totalCount;
            }

            public string Type { get; set; }

            public string PaginationToken { get; set; }

            public long ProcessedCount { get; set; }

            public long TotalCount { get; set; }
        }
    }
}
