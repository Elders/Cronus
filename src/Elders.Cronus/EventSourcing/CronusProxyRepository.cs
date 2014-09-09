//using System;
//using System.Collections.Generic;
//using Elders.Cronus.DomainModeling;

//namespace Elders.Cronus.EventSourcing
//{
//    public class CronusProxyRepository : IAggregateRepository
//    {
//        public List<DomainMessageCommit> Commits { get; set; }

//        private readonly IAggregateRepository aggregateRepository;

//        private IPublisher<IEvent> eventPublisher;

//        public CronusProxyRepository(IAggregateRepository aggregateRepository)
//        {
//            this.aggregateRepository = aggregateRepository;
//            Commits = new List<DomainMessageCommit>();
//        }

//        public void Save(IAggregateRoot aggregateRoot)
//        {
//            if (aggregateRoot.UncommittedEvents == null || aggregateRoot.UncommittedEvents.Count == 0)
//                return;
//            aggregateRoot.State.Version += 1;
//            var commit = new DomainMessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents);
//            Commits.Add(commit);
//        }

//        public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
//        {
//            throw new NotImplementedException();
//        }

//        public AR Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
