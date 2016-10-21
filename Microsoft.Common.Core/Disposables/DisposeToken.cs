// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using static System.FormattableString;

namespace Microsoft.Common.Core.Disposables {
    public sealed class DisposeToken {
        private readonly string _message;
        private int _disposed;

        public static DisposeToken Create<T>() where T : IDisposable {
            return new DisposeToken(Invariant($"{typeof(T).Name} instance is disposed"));
        }

        public bool IsDisposed => _disposed == 1;

        public DisposeToken(string message = null) {
            _message = message;
        }

        public void ThrowIfDisposed() {
            if (_disposed == 0) {
                return;
            }

            if (_message == null) {
                throw new InvalidOperationException();
            }

            throw new InvalidOperationException(_message);
        }

        public bool TryMarkDisposed() {
            return Interlocked.Exchange(ref _disposed, 1) == 0;
        }
    }
}