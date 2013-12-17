using System;
using System.Threading;
using System.Threading.Tasks;

namespace NMSD.Cronus.Core
{
    public static class Threading
    {
        public static Task<T> RunAsync<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();

            ThreadPool.QueueUserWorkItem(delegate
            {
                try { tcs.SetResult(func()); }
                catch (Exception ex) { tcs.SetException(ex); }
            });

            return tcs.Task;
        }
    }
}