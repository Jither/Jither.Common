using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jither.Tasks
{
	public class MessagingSynchronizationContext : SynchronizationContext
	{
		private struct Message
		{
			public readonly SendOrPostCallback Callback;
			public readonly object State;
			public readonly ManualResetEventSlim FinishedEvent;
			
			public Message(SendOrPostCallback callback, object state, ManualResetEventSlim finishedEvent)
			{
				Callback = callback;
				State = state;
				FinishedEvent = finishedEvent;
			}

			public Message(SendOrPostCallback callback, object state) : this(callback, state, null)
			{
			}
		}

        private readonly MessageQueue<Message> queue = new MessageQueue<Message>();
		
		/// <summary>
		/// Sends a message and does not wait
		/// </summary>
		/// <param name="callback">The delegate to execute</param>
		/// <param name="state">The state associated with the message</param>
		public override void Post(SendOrPostCallback callback, object state)
		{
			queue.Post(new Message(callback, state));
		}

		/// <summary>
		/// Sends a message and waits for completion
		/// </summary>
		/// <param name="callback">The delegate to execute</param>
		/// <param name="state">The state associated with the message</param>
		public override void Send(SendOrPostCallback callback, object state)
		{
			var ev = new ManualResetEventSlim(false);
			try
			{
				queue.Post(new Message(callback, state, ev));
				ev.Wait();
			}
			finally
			{
				ev.Dispose();
			}
		}

		/// <summary>
		/// Starts message loop
		/// </summary>
		public void Start(CancellationToken cancellationToken)
		{
			Message msg;
			do
			{
				// blocks until a message comes in:
				msg = queue.Receive(cancellationToken);
				// execute the code on this thread
				msg.Callback?.Invoke(msg.State);

				msg.FinishedEvent?.Set();
			}
			while (null != msg.Callback && !cancellationToken.IsCancellationRequested);
			
			cancellationToken.ThrowIfCancellationRequested();
		}

		/// <summary>
		/// Starts the message loop
		/// </summary>
		public void Start()
		{
			Message msg;
			do
			{
				// blocks until a message comes in:
				msg = queue.Receive();

				msg.Callback?.Invoke(msg.State);

				msg.FinishedEvent?.Set();
			}
			while (msg.Callback != null);
		}
		/// <summary>
		/// Stops the message loop
		/// </summary>
		public void Stop()
		{
			var ev = new ManualResetEventSlim(false);
			try
			{
				// post the quit message
				queue.Post(new Message(null, null, ev));
				ev.Wait();
			}
			finally
			{
				ev.Dispose();
			}
		}
	}
}
