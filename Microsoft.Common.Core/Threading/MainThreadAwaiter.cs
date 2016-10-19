// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Common.Core.Threading {
    public struct MainThreadAwaiter : ICriticalNotifyCompletion {
        private readonly IMainThread _mainThread;

        public MainThreadAwaiter(IMainThread mainThread) {
            _mainThread = mainThread;
        }

        public bool IsCompleted => Thread.CurrentThread.ManagedThreadId == _mainThread.ThreadId;

        public void OnCompleted(Action continuation) {
            Trace.Assert(continuation != null);
            _mainThread.Post(continuation);
        }

        public void UnsafeOnCompleted(Action continuation) {
            Trace.Assert(continuation != null);
            _mainThread.Post(continuation);
        }

        public void GetResult() {
            if (Thread.CurrentThread.ManagedThreadId != _mainThread.ThreadId) {
                throw new InvalidOperationException();
            }
        }
    }
}