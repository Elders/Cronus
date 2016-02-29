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
            for (int expectedRevision = 1; expectedRevision <= eventStream.aggregateCommits.Count; expectedRevision++)
            {
                if (eventStream.aggregateCommits[expectedRevision - 1].Revision != expectedRevision)
                {
                    if (log.IsDebugEnabled())
                    {
                        yield return $"Unordered event stream.";
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

    public class UnorderedRevisionsResolver : IResolver<EventStream>
    {
        public uint PriorityLevel { get { return 100; } }

        public int CompareTo(IResolver<EventStream> other)
        {
            return PriorityLevel.CompareTo(other.PriorityLevel);
        }

        public IntegrityResult<EventStream> Resolve(EventStream eventStream, IValidatorResult validatorResult)
        {
            var orderedRevisions = eventStream.aggregateCommits.OrderBy(x => x.Revision);
            var result = new IntegrityResult<EventStream>(new EventStream(orderedRevisions.ToList()), false);
            return result;
        }
    }
}