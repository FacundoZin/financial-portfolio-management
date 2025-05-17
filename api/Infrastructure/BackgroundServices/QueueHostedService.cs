using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Interfaces.Infrastructure.BackgrounServices;

namespace api.Infrastructure.HostedServices
{
    public class QueueHostedService : BackgroundService
    {
        private readonly ILogger<QueueHostedService> _logger;
        private readonly IBackgroundTaskQueue _taskQueue;
        public QueueHostedService(ILogger<QueueHostedService> logger, IBackgroundTaskQueue taskQueue)
        {
            _logger = logger;
            _taskQueue = taskQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _logger.LogInformation("Queued Hosted Service is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                try
                {
                    if (workItem != null)
                        await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing background task.");
                }
            }

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}