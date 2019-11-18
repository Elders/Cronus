using Elders.Cronus.Hosting.HostOptions;
using Machine.Specifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Elders.Cronus.Configurations
{
    [Subject("Configurations")]
    public class When_configuring_CronusOptions_from__IConfiguration
    {
        Establish context = () =>
        {
            boundedContext = "test";

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            var settings = new Dictionary<string, string>
            {
                {"Cronus:BoundedContext", boundedContext}
            };
            configurationBuilder.AddInMemoryCollection(settings);
            IConfiguration configuration = configurationBuilder.Build();

            optionsProvider = new CronusOptionsProvider(configuration);
        };

        Because of = () => options = optionsProvider.Create(Options.DefaultName);

        It should_have_configured_bounded_context = () => options.BoundedContext.ShouldEqual(boundedContext);

        static string boundedContext;
        static CronusOptionsProvider optionsProvider;
        static CronusOptions options;
    }
}
