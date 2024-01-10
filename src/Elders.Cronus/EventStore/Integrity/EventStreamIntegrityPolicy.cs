using System.Collections.Generic;
using Elders.Cronus.IntegrityValidation;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.EventStore.Integrity
{
    public sealed class EventStreamIntegrityPolicy : IIntegrityPolicy<EventStream>
    {
        readonly List<IntegrityRule<EventStream>> rules;

        public EventStreamIntegrityPolicy()
        {
            rules =
            [
                new IntegrityRule<EventStream>(new DuplicateRevisionsValidator()),
                new IntegrityRule<EventStream>(new OrderedRevisionsValidator(), new UnorderedRevisionsResolver()),
                new IntegrityRule<EventStream>(new MissingRevisionsValidator()),
            ];
        }

        public IEnumerable<IntegrityRule<EventStream>> Rules => rules;

        public IntegrityResult<EventStream> Apply(EventStream eventStream)
        {
            var integrity = new IntegrityResult<EventStream>(eventStream, false);

            foreach (IntegrityRule<EventStream> rule in rules)
            {
                IValidatorResult validatorResult = rule.Validator.Validate(eventStream);
                if (validatorResult.IsValid == false)
                    integrity = rule.Resolver.Resolve(eventStream, validatorResult);
            }

            return integrity;
        }

        class EventStreamValidatorLogger : IResolver<EventStream>
        {
            private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(EventStreamValidatorLogger));

            public uint PriorityLevel { get { return uint.MinValue; } }

            public int CompareTo(IResolver<EventStream> other)
            {
                return PriorityLevel.CompareTo(other.PriorityLevel);
            }

            public IntegrityResult<EventStream> Resolve(EventStream eventStream, IValidatorResult validatorResult)
            {
                if (validatorResult.IsValid == false)
                    logger.Error(() => "EventStream integrity violation occured.");

                if (logger.IsDebugEnabled())
                {
                    foreach (var errorMessage in validatorResult.Errors)
                    {
                        logger.Debug(() => $"[INTEGRITY-ERROR: {validatorResult.ErrorType}] {errorMessage}");
                    }

                    logger.Debug(() => eventStream.ToString());
                }

                return new IntegrityResult<EventStream>(eventStream, true);
            }
        }
    }
}
