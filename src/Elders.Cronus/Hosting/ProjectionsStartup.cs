using System;
using Elders.Cronus.FaultHandling;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Workflow;
using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Elders.Cronus.EventStore.Index;

namespace Elders.Cronus
{
    public class ProjectionsStartup : StartupBase<IProjection>
    {
        private readonly ITenantList tenants;
        private readonly ProjectionHasher hasher;
        private readonly IPublisher<ICommand> publisher;
        private readonly IServiceProvider ioc;
        private readonly SubscriberCollection<IProjection> projectionSubscrbers;
        private readonly TypeContainer<IProjection> handlerTypeContainer;
        private readonly TypeContainer<IEvent> eventsContainer;
        private Workflow<HandleContext> projectionsWorkflow;

        public ProjectionsStartup(IServiceProvider ioc, IConsumer<IProjection> consumer, SubscriberCollection<IProjection> projectionSubscrbers, TypeContainer<IProjection> handlerTypeContainer, TypeContainer<IEvent> eventsContainer, ITenantList tenants, ProjectionHasher hasher, IPublisher<ICommand> publisher)
            : base(consumer)
        {
            this.tenants = tenants;
            this.hasher = hasher;
            this.publisher = publisher;
            this.ioc = ioc;
            this.projectionSubscrbers = projectionSubscrbers;
            this.handlerTypeContainer = handlerTypeContainer;
            this.eventsContainer = eventsContainer;
            RegisterProjections();
            var messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
            var scopedWorkflow = new ScopedMessageWorkflow(ioc, messageHandleWorkflow);
            messageHandleWorkflow.Finalize.Use(new ProjectionsWorkflow(x => ScopedMessageWorkflow.GetScope(x).ServiceProvider.GetRequiredService<IProjectionWriter>()));
            projectionsWorkflow = new InMemoryRetryWorkflow<HandleContext>(scopedWorkflow);

            RegisterSubscribers();
        }

        void RegisterSubscribers()
        {
            foreach (var type in handlerTypeContainer.Items)
            {
                var handlerSubscriber = new HandlerSubscriber(type, projectionsWorkflow);
                projectionSubscrbers.Subscribe(handlerSubscriber);
            }
            var indexSubscriber = new IndexByEventTypeSubscriber(eventsContainer, ioc, scope => scope.ServiceProvider.GetRequiredService<IIndexStore>());
            projectionSubscrbers.Subscribe(indexSubscriber);
        }

        void RegisterProjections()
        {
            foreach (var handler in handlerTypeContainer.Items)
            {
                foreach (var tenant in tenants.GetTenants())
                {
                    var id = new ProjectionVersionManagerId(handler.GetContractId(), tenant);
                    var command = new RegisterProjection(id, hasher.CalculateHash(handler));
                    publisher.Publish(command);
                }
            }
        }
    }
}
