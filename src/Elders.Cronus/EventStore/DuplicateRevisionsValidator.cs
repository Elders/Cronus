using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.Logging;

namespace Elders.Cronus.EventStore
{
    public class DuplicateRevisionsValidator : IValidator<EventStream>
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(OrderedRevisionsValidator));

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
                if (log.IsDebugEnabled())
                {
                    yield return $"Found {error.Count().ToString()} duplicates of {nameof(AggregateCommit)} for revision {error.Key.ToString()}.";
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
