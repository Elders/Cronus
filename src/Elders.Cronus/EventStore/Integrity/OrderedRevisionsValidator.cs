using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IntegrityValidation;

namespace Elders.Cronus.EventStore.Integrity;

public sealed class OrderedRevisionsValidator : IValidator<EventStream>
{
    public uint PriorityLevel => 200;

    public int CompareTo(IValidator<EventStream> other)
    {
        return PriorityLevel.CompareTo(other.PriorityLevel);
    }

    public IValidatorResult Validate(EventStream candidate)
    {
        return new ValidatorResult(GetErrorMessages(candidate), nameof(OrderedRevisionsValidator));
    }

    private IEnumerable<string> GetErrorMessages(EventStream eventStream)
    {
        int previousRevision = 0;
        foreach (AggregateCommit current in eventStream.Commits)
        {
            if (previousRevision > current.Revision)
                yield return $"Unordered event stream. Expected revision `{previousRevision}` but received revision `{current.Revision}`";

            previousRevision = current.Revision;
        }
    }
}

public sealed class UnorderedRevisionsResolver : IResolver<EventStream>
{
    public uint PriorityLevel { get { return 100; } }

    public int CompareTo(IResolver<EventStream> other)
    {
        return PriorityLevel.CompareTo(other.PriorityLevel);
    }

    public IntegrityResult<EventStream> Resolve(EventStream eventStream, IValidatorResult validatorResult)
    {
        var orderedRevisions = eventStream.Commits.OrderBy(x => x.Revision);
        var result = new IntegrityResult<EventStream>(new EventStream(orderedRevisions.ToList()), false);
        return result;
    }
}
