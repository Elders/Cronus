﻿using Elders.Cronus.EventStore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.MessageProcessing
{
    public class AggregateCommitPublisherRepository : IAggregateRepository
    {
        readonly AggregateRepository aggregateRepository;
        private readonly ICronusContextAccessor contextAccessor;
        private readonly IPublisher<AggregateCommit> commiter;
        private readonly ILogger<AggregateCommitPublisherRepository> logger;

        public AggregateCommitPublisherRepository(AggregateRepository repository, IPublisher<AggregateCommit> commiter, ICronusContextAccessor contextAccessor, ILogger<AggregateCommitPublisherRepository> logger)
        {
            this.aggregateRepository = repository;
            this.contextAccessor = contextAccessor;
            this.logger = logger;
            this.commiter = commiter;
        }

        public Task<ReadResult<AR>> LoadAsync<AR>(AggregateRootId id) where AR : IAggregateRoot
        {
            return aggregateRepository.LoadAsync<AR>(id);
        }

        public async Task SaveAsync<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            AggregateCommit commit = await aggregateRepository.SaveInternalAsync(aggregateRoot).ConfigureAwait(false);

            if (commit is default(AggregateCommit))
            {
                logger.Debug(() => "Aggregate commit has not been persisted and no events have been published because the `source` persistence action did not finish successfully. This usually happens when the AR did not generate any new events such as ignoring a command. (It is fine but check your business logic.)");
                return;
            }

            try
            {
                bool publishResult = commiter.Publish(commit, BuildHeaders(commit));

                if (publishResult == false)
                    logger.Error(() => "Unable to publish aggregate commit.");
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

            foreach (var trace in contextAccessor.CronusContext.Trace)
            {
                messageHeaders.Add(trace.Key, trace.Value.ToString());
            }

            return messageHeaders;
        }
    }
}
