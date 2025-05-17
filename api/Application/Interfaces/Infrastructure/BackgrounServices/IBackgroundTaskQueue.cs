using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Application.Interfaces.Infrastructure.BackgrounServices
{
    public interface IBackgroundTaskQueue
    {
        void Enqueue(Func<CancellationToken, Task> workItem);
        ValueTask<Func<CancellationToken, Task>?> DequeueAsync(CancellationToken cancellationToken);
    }
}