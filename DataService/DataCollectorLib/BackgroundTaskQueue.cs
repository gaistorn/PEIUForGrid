using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.DataServices
{
    public interface IBackgroundTaskQueue<T>
    {
        void QueueWorkItem(T item);
        Task<T> DequeueAsync(CancellationToken cancellationToken);
        int QueueCount { get; }
    }
    public class BackgroundTaskQueue<T> : IBackgroundTaskQueue<T>
    {
        private ConcurrentQueue<T> _workItems =
        new ConcurrentQueue<T>();
        //readonly MongoDB.Driver.MongoClient client;
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public int QueueCount
        {
            get
            {
                return _workItems.Count;
            }
        }

        public BackgroundTaskQueue()
        {

        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;

        }

        public void QueueWorkItem(T workItem)
        {
            //   DaegunPacket workItem = new DaegunPacket(client, packetStruct);
            _workItems.Enqueue(workItem);
            _signal.Release();

        }
    }
}
