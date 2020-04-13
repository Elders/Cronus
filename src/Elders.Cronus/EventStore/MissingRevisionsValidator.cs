﻿using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IntegrityValidation;

namespace Elders.Cronus.EventStore
{
    public class MissingRevisionsValidator : IValidator<EventStream>
    {
        public uint PriorityLevel { get { return 300; } }

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
            int maxRevision = eventStream.Commits.Max(r => r.Revision);

            for (int expectedRevision = 1; expectedRevision <= maxRevision; expectedRevision++)
            {
                if (eventStream.Commits.Any(x => x.Revision == expectedRevision) == false)
                {
                    yield return $"Missing {nameof(AggregateCommit)} revision '{expectedRevision}'.";
                }
            }
        }
    }
}
