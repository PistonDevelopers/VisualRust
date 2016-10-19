// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static System.FormattableString;

namespace Microsoft.Common.Core.Tasks {
    public class TaskCompletionSourceEx<TResult> {
        private readonly AsyncTaskMethodBuilder<TResult> _atmb;
        private int _completed;

        public Task<TResult> Task { get; }

        public TaskCompletionSourceEx() {
            _atmb = AsyncTaskMethodBuilder<TResult>.Create();
            Task = _atmb.Task;
        }

        public bool TrySetResult(TResult result) {
            if (Task.IsCompleted) {
                return false;
            }

            if (Interlocked.CompareExchange(ref _completed, 1, 0) == 0) {
                _atmb.SetResult(result);
                return true;
            }

            SpinUntilCompleted();
            return false;
        }

        public bool TrySetCanceled(OperationCanceledException exception = null, CancellationToken cancellationToken = default(CancellationToken)) {
            if (Task.IsCompleted) {
                return false;
            }

            if (Interlocked.CompareExchange(ref _completed, 1, 0) == 0) {
                exception = exception ?? new OperationCanceledException(cancellationToken);
                _atmb.SetException(exception);
                return true;
            }

            SpinUntilCompleted();
            return false;
        }

        public bool TrySetException(Exception exception) {
            if (exception == null) {
                throw new ArgumentNullException(nameof(exception));
            }

            if (exception is OperationCanceledException) {
                throw new ArgumentOutOfRangeException(nameof(exception), Invariant($"Use {nameof(TrySetCanceled)} to cancel task"));
            }

            if (Task.IsCompleted) {
                return false;
            }
            
            if (Interlocked.CompareExchange(ref _completed, 1, 0) == 0) {
                _atmb.SetException(exception);
                return true;
            }

            SpinUntilCompleted();
            return false;
        }

        private void SpinUntilCompleted() {
            if (Task.IsCompleted) {
                return;
            }

            var sw = new SpinWait();
            while (!Task.IsCompleted) {
                sw.SpinOnce();
            }
        }

        public void SetResult(TResult result) {
            if (!TrySetResult(result)) {
                throw new InvalidOperationException("Task already completed");
            }
        }

        public void SetCanceled(OperationCanceledException exception = null, CancellationToken cancellationToken = default(CancellationToken)) {
            if (!TrySetCanceled(exception, cancellationToken)) {
                throw new InvalidOperationException("Task already completed");
            }
        }

        public void SetException(Exception exception) {
            if (!TrySetException(exception)) {
                throw new InvalidOperationException("Task already completed");
            }
        }
    }
}