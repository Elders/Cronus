﻿using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Hosting.Heartbeat
{
    public interface IHeartbeat
    {
        Task StartBeating(CancellationToken stoppingToken);
    }
}
