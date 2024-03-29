﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.AtomicAction.InMemory;

public class InMemoryLock : ILock
{
    private readonly ILogger logger;

    public InMemoryLock(ILogger logger)
    {
        this.logger = logger;
    }

    private static readonly ConcurrentDictionary<string, Mutex> lockedResources = new ConcurrentDictionary<string, Mutex>();

    public Task<bool> IsLockedAsync(string resource)
    {
        Mutex locked;

        if (lockedResources.TryGetValue(resource, out locked) == false)
            return Task.FromResult(false);

        return Task.FromResult(locked.IsLocked);
    }

    public Task<bool> LockAsync(string resource, TimeSpan ttl)
    {
        try
        {
            var resourcesToRemove = new List<string>();

            foreach (var item in lockedResources)
            {
                if (item.Value.IsLocked == false)
                    resourcesToRemove.Add(item.Key);
            }

            foreach (var item in resourcesToRemove)
            {
                Mutex l;

                lockedResources.TryRemove(item, out l);
            }

            var locked = new Mutex(resource, ttl);

            lockedResources.AddOrUpdate(resource, locked, (r, old) => locked);

            return Task.FromResult(true);

        }
        catch (Exception ex)
        {
            logger.ErrorException(ex, () => $"Failed to accure lock for resource {resource}!");

            return Task.FromResult(false);
        }
    }

    public Task UnlockAsync(string resource)
    {
        try
        {
            var resourcesToRemove = new List<string>();

            foreach (var item in lockedResources)
            {
                if (item.Value.IsLocked == false)
                    resourcesToRemove.Add(item.Key);
            }

            foreach (var item in resourcesToRemove)
            {
                Mutex l;

                lockedResources.TryRemove(item, out l);
            }

            Mutex m;

            if (lockedResources.TryRemove(resource, out m) == false)
                logger.Error(() => $"Failed to unlock mutex: {resource}!");

        }
        catch (Exception ex)
        {
            logger.ErrorException(ex, () => $"Failed to unlock mutex: {resource}!");
        }

        return Task.CompletedTask;
    }

    private class Mutex
    {
        public string Resource { get; private set; }

        public TimeSpan Ttl { get; private set; }

        public DateTimeOffset CreatedDate { get; private set; }

        public bool IsLocked { get { return DateTime.UtcNow < CreatedDate.Add(Ttl); } }

        public Mutex(string resource, TimeSpan ttl)
        {
            Resource = resource;
            Ttl = ttl;
            CreatedDate = new DateTimeOffset(DateTime.UtcNow);
        }
    }
}
