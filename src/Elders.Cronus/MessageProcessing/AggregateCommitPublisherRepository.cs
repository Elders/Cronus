﻿using Elders.Cronus.EventStore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Elders.Cronus.MessageProcessing
{
    public class AggregateCommitPublisherRepository : IAggregateRepository
    {
        readonly AggregateRepository aggregateRepository;
        private readonly CronusContext context;
        private readonly IPublisher<AggregateCommit> commiter;
        private readonly ILogger<AggregateCommitPublisherRepository> logger;

        public AggregateCommitPublisherRepository(AggregateRepository repository, IPublisher<AggregateCommit> commiter, CronusContext context, ILogger<AggregateCommitPublisherRepository> logger)
        {
            this.aggregateRepository = repository;
            this.context = context;
            this.logger = logger;
            this.commiter = commiter;
        }

        public ReadResult<AR> Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            return aggregateRepository.Load<AR>(id);
        }

        public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            AggregateCommit commit = aggregateRepository.SaveInternal(aggregateRoot);

            if (commit is default(AggregateCommit))
            {
                logger.Debug(() => "Aggregate commit will not be published because the `source` persistence action did not finish successfully. This usually happens when the AR ignores a command. (It is fine but check your business logic.)");
                return;
            }

            try
            {
                commiter.Publish(commit, BuildHeaders(commit));
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => "Unable to publish aggregate commit.");
            }
        }

        Dictionary<string, string> BuildHeaders(AggregateCommit commit)
        {
            Dictionary<string, string> messageHeaders = new Dictionary<string, string>
            {
                { MessageHeader.AggregateRootId, Convert.ToBase64String(commit.AggregateRootId) }
            };

            foreach (var trace in context.Trace)
            {
                messageHeaders.Add(trace.Key, trace.Value.ToString());
            }

            return messageHeaders;
        }
    }
}
