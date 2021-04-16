using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jither.Tasks
{
	/// <summary>
	/// Thread-safe asynchronous message queue.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class MessageQueue<T>
	{
		private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim sync = new SemaphoreSlim(0);

		public bool IsEmpty => queue.IsEmpty;
		
		public int Count => queue.Count;

		public void Post(T message)
		{
			queue.Enqueue(message);
			sync.Release(1);
		}

		public T Receive()
		{
            sync.Wait();

            if (!queue.TryDequeue(out T result))
			{
				throw new InvalidOperationException("The queue is empty");
			}
			
			return result;
		}

		public T Receive(CancellationToken cancellationToken)
		{
            sync.Wait(cancellationToken);
            
			cancellationToken.ThrowIfCancellationRequested();

			if (!queue.TryDequeue(out T result))
			{
				throw new InvalidOperationException("The queue is empty");
			}
			
			cancellationToken.ThrowIfCancellationRequested();
			
			return result;
		}


		public async Task<T> ReceiveAsync()
		{
			await sync.WaitAsync();

			if (!queue.TryDequeue(out T result))
			{
				throw new InvalidOperationException("The queue is empty");
			}
			
			return result;
		}
		public async Task<T> ReceiveAsync(CancellationToken cancellationToken)
		{
			await sync.WaitAsync(cancellationToken);

			cancellationToken.ThrowIfCancellationRequested();

			if (!queue.TryDequeue(out T result))
			{
				throw new InvalidOperationException("The queue is empty");
			}

			cancellationToken.ThrowIfCancellationRequested();
			
			return result;
		}

		public bool Poll(out T result)
		{
			return queue.TryDequeue(out result);
		}
	}
}
