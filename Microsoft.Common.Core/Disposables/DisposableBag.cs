// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Common.Core.Disposables {
    public sealed class DisposableBag {
        private readonly DisposeToken _token;
        private readonly ConcurrentStack<Action> _disposables;

        public static DisposableBag Create<T>(IDisposable disposable) where T : IDisposable => Create<T>().Add(disposable);
        public static DisposableBag Create<T>(Action action) where T : IDisposable => Create<T>().Add(action);
        public static DisposableBag Create<T>() where T : IDisposable => new DisposableBag(FormattableString.Invariant($"{typeof(T).Name} instance is disposed"));

        public DisposableBag(string message = null) {
            _token = new DisposeToken(message);
            _disposables = new ConcurrentStack<Action>();
        }

        public DisposableBag Add(IDisposable disposable) => Add(disposable.Dispose);

        public DisposableBag Add(Action action) {
            ThrowIfDisposed();
            _disposables.Push(action);
            return this;
        }

        public void ThrowIfDisposed() => _token.ThrowIfDisposed();
        public bool TryDispose() {
            var disposed = _token.TryMarkDisposed();
            if (disposed) {
                foreach (var disposable in _disposables) {
                    disposable();
                }
                _disposables.Clear();
            }
            return disposed;
        }
    }
}