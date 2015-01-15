using System;
using Elders.Cronus.DomainModeling;
using Machine.Specifications;

namespace Elders.Cronus.Tests.DomainModeling
{
    [Subject("IocContainer")]
    public class When__AggregateRootId__is_created
    {
        Establish context = () =>
            {
                rawIdAsString = "V2hlbl9fQWdncmVnYXRlUm9vdElkX19pc19jcmVhdGVkQB5ZNsjgG2NLlvDxjoZusFE=";
                aggregateRootName = "When__AggregateRootId__is_created";
                id = Guid.Parse("c836591e-1be0-4b63-96f0-f18e866eb051");
            };

        Because of = () => guidId = new GuidId(id, aggregateRootName);

        It should_have_well_formatted_string_message = () => guidId.ToString().ShouldEqual(aggregateRootName + "@" + id.ToString() + "||" + rawIdAsString);


        static IAggregateRootId guidId;
        static Guid id;
        static string aggregateRootName;
        static string rawIdAsString;
    }
}