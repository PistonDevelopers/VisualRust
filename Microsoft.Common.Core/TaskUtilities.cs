// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Threading;
using static System.FormattableString;

namespace Microsoft.Common.Core {
    public static class TaskUtilities {
        public static Task CreateCanceled(OperationCanceledException exception = null) {
            exception = exception ?? new OperationCanceledException();
            var atmb = new AsyncTaskMethodBuilder();
            atmb.SetException(exception);
            return atmb.Task;
        }

        public static Task<TResult> CreateCanceled<TResult>(OperationCanceledException exception = null) {
            exception = exception ?? new OperationCanceledException();
            var atmb = new AsyncTaskMethodBuilder<TResult>();
            atmb.SetException(exception);
            return atmb.Task;
        }

        public static bool IsOnBackgroundThread() {
            var taskScheduler = TaskScheduler.Current;
            var syncContext = SynchronizationContext.Current;
            return taskScheduler == TaskScheduler.Default && (syncContext == null || syncContext.GetType() == typeof(SynchronizationContext));
        }

        /// <summary>
        /// If awaited on a thread with custom scheduler or synchronization context, invokes the continuation
        /// on a background (thread pool) thread. If already on such a thread, await is a no-op.
        /// </summary>
        public static BackgroundThreadAwaitable SwitchToBackgroundThread() {
            return new BackgroundThreadAwaitable();
        }

        [Conditional("TRACE")]
        public static void AssertIsOnBackgroundThread(
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        ) {
            if (!IsOnBackgroundThread()) {
                Trace.Fail(Invariant($"{memberName} at {sourceFilePath}:{sourceLineNumber} was incorrectly called from a non-background thread."));
            }
        }

        public static Task WhenAllCancelOnFailure(params Func<CancellationToken, Task>[]  functions) => WhenAllCancelOnFailure(functions, default(CancellationToken));

        public static Task WhenAllCancelOnFailure<TSource>(IEnumerable<TSource> source, Func<TSource, CancellationToken, Task> taskFactory, CancellationToken cancellationToken) {
            var functions = source.Select(s => SourceToFunctionConverter(s, taskFactory));
            return WhenAllCancelOnFailure(functions, cancellationToken);
        }

        private static Func<CancellationToken, Task> SourceToFunctionConverter<TSource>(TSource source, Func<TSource, CancellationToken, Task> taskFactory)
            => ct => taskFactory(source, ct);

        public static Task WhenAllCancelOnFailure(IEnumerable<Func<CancellationToken, Task>> functions, CancellationToken cancellationToken) {
            var functionsList = functions.ToList();
            var cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSourceEx<bool>();
            var state = new WhenAllCancelOnFailureContinuationState(functionsList.Count, tcs, cts);

            foreach (var function in functionsList) {
                var task = function(CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token);
                task.ContinueWith(WhenAllCancelOnFailureContinuation, state, default(CancellationToken), TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }
            
            return tcs.Task;
        }

        private static void WhenAllCancelOnFailureContinuation(Task task, object state) {
            var continuationState = (WhenAllCancelOnFailureContinuationState) state;
            switch (task.Status) {
                case TaskStatus.RanToCompletion:
                    if (Interlocked.Decrement(ref continuationState.Count) == 0) {
                        continuationState.TaskCompletionSource.TrySetResult(true);
                    }
                    break;
                case TaskStatus.Canceled:
                    try {
                        task.GetAwaiter().GetResult();
                    } catch (OperationCanceledException ex) {
                        if (continuationState.TaskCompletionSource.TrySetCanceled(ex)) {
                            continuationState.CancellationTokenSource.Cancel();
                        }
                    }
                    break;
                case TaskStatus.Faulted:
                    if (continuationState.TaskCompletionSource.TrySetException(task.Exception)) {
                        continuationState.CancellationTokenSource.Cancel();
                    }
                    break;
            }
        }

        private class WhenAllCancelOnFailureContinuationState {
            public int Count;
            public readonly TaskCompletionSourceEx<bool> TaskCompletionSource;
            public readonly CancellationTokenSource CancellationTokenSource;

            public WhenAllCancelOnFailureContinuationState(int count, TaskCompletionSourceEx<bool> taskCompletionSource, CancellationTokenSource cancellationTokenSource) {
                Count = count;
                TaskCompletionSource = taskCompletionSource;
                CancellationTokenSource = cancellationTokenSource;
            }
        }
    }
}
