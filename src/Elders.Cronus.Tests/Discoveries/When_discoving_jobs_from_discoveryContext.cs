using Elders.Cronus.Cluster.Job;
using Elders.Cronus.Cluster.Job.InMemory;
using Elders.Cronus.EventStore.Index;
using Machine.Specifications;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Discoveries
{
    [Subject("Discoveries")]
    public class When_discoving_jobs_from_discoveryContext
    {
        Establish context = () =>
        {
            IConfiguration configuration = new ConfigurationMock();

            discoveryContext = new DiscoveryContext(new List<Assembly> { typeof(When_discoving_jobs_from_discoveryContext).Assembly }, configuration);
        };

        Because of = () => result = new JobDiscovery().Discover(discoveryContext);

        It should_have_jobs_discovered = () => result.Models.Count().ShouldBeGreaterThan(0);

        It should_have_correct_job_discovered = () => result.Models.ShouldContain(x => x.ServiceType == typeof(TestJob) && x.ImplementationType == typeof(TestJob) && x.Lifetime.Equals(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient));

        It should_have_job_type_container = () => result.Models.ShouldContain(x => x.ServiceType == typeof(TypeContainer<ICronusJob<object>>) && x.ImplementationInstance.GetType().Equals(typeof(TypeContainer<ICronusJob<object>>)) && x.Lifetime.Equals(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));

        static IDiscoveryResult<ICronusJob<object>> result;
        static DiscoveryContext discoveryContext;
    }

    public class TestJobData : IJobData
    {
        public bool IsCompleted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTimeOffset Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class TestJob : CronusJob<TestJobData>
    {
        public override string Name { get; set; } = "Test";

        public TestJob() : base(null)
        {

        }

        protected override TestJobData BuildInitialData()
        {
            throw new NotImplementedException();
        }

        protected override Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
