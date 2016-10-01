// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;

namespace Microsoft.Common.Core.Threading {
    public class AsyncCountdownEvent {
        private readonly AsyncManualResetEvent _mre = new AsyncManualResetEvent();
        private int _count;

        public AsyncCountdownEvent(int initialCount) {
            if (initialCount < 0) {
                throw new ArgumentOutOfRangeException(nameof(initialCount));
            }

            _count = initialCount;
            if (initialCount == 0) {
                _mre.Set();
            }
        }

        public Task WaitAsync() => _mre.WaitAsync();

        public Task WaitAsync(CancellationToken cancellationToken) => _mre.WaitAsync(cancellationToken);

        public void Signal() {
            if (_count <= 0) {
                throw new InvalidOperationException();
            }

            var count = Interlocked.Decrement(ref _count);
            if (count < 0) {
                throw new InvalidOperationException();
            }

            if (count == 0) {
                _mre.Set();
            }
        }

        public void AddOne() {
            _mre.Reset();
            Interlocked.Increment(ref _count);
        }

        public IDisposable AddOneDisposable() {
            AddOne();
            return Disposable.Create(Signal);
        }
    }
}