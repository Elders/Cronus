using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.Logging;

namespace Elders.Cronus.EventStore
{
    public class EventStreamIntegrityPolicy : IIntegrityPolicy<EventStream>
    {
        static EventStreamValidatorLogger logger = new EventStreamValidatorLogger();

        readonly HashSet<IntegrityRule<EventStream>> rules;

        public EventStreamIntegrityPolicy()
        {
            rules = new HashSet<IntegrityRule<EventStream>>();
        }

        public IEnumerable<IntegrityRule<EventStream>> Rules { get { return rules.ToList().AsReadOnly(); } }

        public void RegisterRule(IntegrityRule<EventStream> rule)
        {
            rules.Add(rule);
            rules.Add(new IntegrityRule<EventStream>(rule.Validator, logger));
        }

        public IntegrityResult<EventStream> Apply(EventStream eventStream)
        {
            var integrity = new IntegrityResult<EventStream>(eventStream, false);

            var rulesByValidator = rules.GroupBy(x => x.Validator).OrderBy(x => x.Key);

            foreach (var rulesPerValidator in rulesByValidator)
            {
                var validatorResult = rulesPerValidator.Key.Validate(eventStream);
                if (validatorResult.IsValid)
                    continue;

                foreach (var rule in rulesPerValidator.OrderBy(x => x.Resolver.PriorityLevel))
                {
                    integrity = rule.Resolver.Resolve(eventStream, validatorResult);
                    if (integrity.IsIntegrityViolated == false)
                        break;
                }
            }

            return integrity;
        }

        public static EventStreamIntegrityPolicy Merge(EventStreamIntegrityPolicy first, EventStreamIntegrityPolicy second)
        {
            var mergedPolicy = new EventStreamIntegrityPolicy();
            var combinedRules = first.Rules.Union(second.Rules);
            foreach (var rule in combinedRules)
            {
                mergedPolicy.RegisterRule(rule);
            }

            return mergedPolicy;
        }

        class EventStreamValidatorLogger : IResolver<EventStream>
        {
            static readonly ILog log = LogProvider.GetLogger(typeof(EventStreamValidatorLogger));

            public uint PriorityLevel { get { return uint.MinValue; } }

            public int CompareTo(IResolver<EventStream> other)
            {
                return PriorityLevel.CompareTo(other.PriorityLevel);
            }

            public IntegrityResult<EventStream> Resolve(EventStream eventStream, IValidatorResult validatorResult)
            {
                if (validatorResult.IsValid == false)
                    log.Error(() => "EventStream integrity violation occured.");

                if (log.IsDebugEnabled())
                {
                    foreach (var errorMessage in validatorResult.Errors)
                    {
                        log.Debug($"[INTEGRITY-ERROR: {validatorResult.ErrorType}] {errorMessage}");
                    }
                    log.Debug(eventStream.ToString());
                }

                return new IntegrityResult<EventStream>(eventStream, true);
            }
        }
    }
}