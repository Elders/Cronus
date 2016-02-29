using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.Logging;

namespace Elders.Cronus.EventStore
{
    public class MissingRevisionsValidator : IValidator<EventStream>
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(OrderedRevisionsValidator));

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
            int maxRevision = eventStream.aggregateCommits.Max(r => r.Revision);

            for (int expectedRevision = 1; expectedRevision <= maxRevision; expectedRevision++)
            {
                if (eventStream.aggregateCommits.Any(x => x.Revision == expectedRevision) == false)
                {
                    if (log.IsDebugEnabled())
                    {
                        yield return $"Missing {nameof(AggregateCommit)} revision '{expectedRevision}'.";
                    }
                    else
                    {
                        yield return "DEBUG logging is turned off. In order to see detailed messages please enable DEBUG log level.";
                        break;
                    }
                }
            }
        }
    }
}