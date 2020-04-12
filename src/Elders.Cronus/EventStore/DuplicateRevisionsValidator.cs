using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IntegrityValidation;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.EventStore
{
    public class DuplicateRevisionsValidator : IValidator<EventStream>
    {
        readonly ILogger logger = CronusLogger.CreateLogger(typeof(OrderedRevisionsValidator));

        public uint PriorityLevel { get { return 100; } }

        public int CompareTo(IValidator<EventStream> other)
        {
            return PriorityLevel.CompareTo(other.PriorityLevel);
        }

        public IValidatorResult Validate(EventStream candidate)
        {
            var possibleConflicts = candidate.Commits
                .GroupBy(x => x.Revision).ToList()
                .Where(g => g.Count() > 1);

            return new ValidatorResult(GetErrorMessages(possibleConflicts), nameof(DuplicateRevisionsValidator));
        }

        private IEnumerable<string> GetErrorMessages(IEnumerable<IGrouping<int, AggregateCommit>> errors)
        {
            foreach (var error in errors)
            {
                if (logger.IsDebugEnabled())
                {
                    yield return $"Found {error.Count()} duplicates of {nameof(AggregateCommit)} for revision {error.Key}.";
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
