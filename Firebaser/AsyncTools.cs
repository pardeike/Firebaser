using System;
using System.Threading;

namespace Firebaser
{
	public class AsyncTools
	{
		public static T RunActionWithTimeout<T>(Func<T> func, int timeout)
		{
			var result = default(T);
			var waitHandle = new AutoResetEvent(false);
			ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object state) { result = func(); waitHandle.Set(); }), waitHandle);
			WaitHandle.WaitAny(new[] { waitHandle }, timeout);
			return result;
		}

		public static T RunFuncWithTimeout<S, T>(Func<S, T> func, S input, int timeout)
		{
			var result = default(T);
			var waitHandle = new AutoResetEvent(false);
			ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object state) { result = func(input); waitHandle.Set(); }), waitHandle);
			WaitHandle.WaitAny(new[] { waitHandle }, timeout);
			return result;
		}
	}
}