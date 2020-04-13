using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IntegrityValidation;

namespace Elders.Cronus.EventStore
{
    public class DuplicateRevisionsValidator : IValidator<EventStream>
    {
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
                yield return $"Found {error.Count()} duplicates of {nameof(AggregateCommit)} for revision {error.Key}.";
            }
        }
    }
}
