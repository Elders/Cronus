using System.Collections.Generic;

namespace Elders.Cronus.EventStore;

public class LoadAggregateCommitsResult
{
    public LoadAggregateCommitsResult()
    {
        Commits = new List<AggregateCommit>();
    }

    public string PaginationToken { get; set; }

    public List<AggregateCommit> Commits { get; set; }

    public bool HasMoreResults { get; set; }
}
