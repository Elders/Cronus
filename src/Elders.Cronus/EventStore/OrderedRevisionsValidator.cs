using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.Logging;

namespace Elders.Cronus.EventStore
{
    public class OrderedRevisionsValidator : IValidator<EventStream>
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(OrderedRevisionsValidator));

        public uint PriorityLevel { get { return 200; } }

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
            List<AggregateCommit> commits = eventStream.Commits.ToList();

            int previousRevision = 0;
            foreach (var current in commits)
            {
                if (previousRevision > current.Revision)
                {
                    if (log.IsDebugEnabled())
                    {
                        yield return $"Unordered event stream. Expected revision `{previousRevision}` but received revision `{current.Revision}`";
                    }
                    else
                    {
                        yield return "DEBUG logging is turned off. In order to see detailed messages please enable DEBUG log level.";
                        break;
                    }
                }
                previousRevision = current.Revision;
            }
        }
    }

    public class UnorderedRevisionsResolver : IResolver<EventStream>
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
}