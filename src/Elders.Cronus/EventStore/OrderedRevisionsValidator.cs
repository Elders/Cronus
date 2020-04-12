using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IntegrityValidation;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.EventStore
{
    public class OrderedRevisionsValidator : IValidator<EventStream>
    {
        readonly ILogger logger = CronusLogger.CreateLogger(typeof(OrderedRevisionsValidator));

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
            int previousRevision = 0;
            foreach (var current in eventStream.Commits)
            {
                if (previousRevision > current.Revision)
                {
                    if (logger.IsDebugEnabled())
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
