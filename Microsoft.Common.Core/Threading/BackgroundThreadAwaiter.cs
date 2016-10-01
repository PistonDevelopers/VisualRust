// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Common.Core.Threading {
    public struct BackgroundThreadAwaiter : ICriticalNotifyCompletion {
        private static readonly WaitCallback WaitCallback = state => ((Action)state)();

        public bool IsCompleted => TaskUtilities.IsOnBackgroundThread();

        public void OnCompleted(Action continuation) {
            Trace.Assert(continuation != null);
            ThreadPool.QueueUserWorkItem(WaitCallback, continuation);
        }

        public void UnsafeOnCompleted(Action continuation) {
            Trace.Assert(continuation != null);
            ThreadPool.UnsafeQueueUserWorkItem(WaitCallback, continuation);
        }

        public void GetResult() {
        }
    }
}