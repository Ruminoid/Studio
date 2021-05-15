using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Ruminoid.Studio.Operations
{
    public interface IOperationDispatcher
    {
        /// <summary>
        /// 执行操作。
        /// </summary>
        /// <param name="operation">要执行的操作。</param>
        /// <remarks>
        /// <para>
        /// 使用 <see cref="OperationDispatcher"/> 执行操作。
        /// </para>
        /// <para>
        /// 这是一个强制行为。不使用 <see cref="OperationDispatcher"/> 将会导致非预期的结果。
        /// </para>
        /// <para>
        /// <paramref name="operation"/> 将会由 <see cref="OperationDispatcher._dispatcherThread"/>
        /// 进行调度并执行。
        /// </para>
        /// </remarks>
        public void Invoke(Action operation);
    }

    public sealed class OperationDispatcher : IOperationDispatcher
    {
        public OperationDispatcher()
        {
            _dispatcherThread = new(Dispatch)
            {
                Name = "Rmstd Operation Dispatcher",
                IsBackground = true
            };

            _dispatcherThread.Start();
        }

        #region Dispatcher Thread

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Thread _dispatcherThread;

        [SuppressMessage("ReSharper", "FunctionNeverReturns")]
        private void Dispatch()
        {
            while (true)
            {
                _dispatchWaitHandle.WaitOne();

                bool result = _dispatchQueue.TryDequeue(out Action operation);
                if (!result) continue;

                operation();

                // 在 <see cref="Avalonia.Threading.Dispatcher.UIThread"/> 上执行
                // Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(operation);
            }
        }

        #endregion

        #region Concurrent Queue

        private readonly ConcurrentQueue<Action> _dispatchQueue = new();
        private readonly AutoResetEvent _dispatchWaitHandle = new(false);

        #endregion

        #region Invoke

        public void Invoke(Action operation)
        {
            _dispatchQueue.Enqueue(operation);
            _dispatchWaitHandle.Set();
        }

        #endregion
    }
}
