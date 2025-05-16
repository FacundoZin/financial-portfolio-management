using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Application.Interfaces.TaskQueue
{
    public interface IBackgroundTaskQueue
    {
        void Enqueue(Func<CancellationToken, Task> workItem);
        ValueTask<Func<CancellationToken, Task>?> DequeueAsync(CancellationToken cancellationToken);
    }
}