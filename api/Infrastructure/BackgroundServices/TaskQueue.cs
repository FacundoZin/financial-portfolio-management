using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using api.Application.Interfaces.Infrastructure.BackgrounServices;

namespace api.Infrastructure.BackgroundServices
{
    public class TaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, Task>> _queue;
        private readonly int capacity = 100;

        public TaskQueue()
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };

            _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(options);
        }

        public void Enqueue(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            _queue.Writer.TryWrite(workItem);
        }

        public async ValueTask<Func<CancellationToken, Task>?> DequeueAsync(CancellationToken _cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(_cancellationToken);
            return workItem;
        }

        public Task DequeueAsync(object stoppingtoken)
        {
            throw new NotImplementedException();
        }
    }
}