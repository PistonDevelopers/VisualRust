// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Threading {
    /// <summary>
    /// BinaryAsyncLock is a helper primitive that can be used instead of SemaphoreSlim.WaitAsync + double-checked locking
    /// </summary>
    /// <remarks>
    /// After BinaryAsyncLock is created or reset, the first caller of <see cref="WaitAsync"/> will immediately get <see cref="IBinaryAsyncLockToken" />
    /// that is not set. All other callers will either wait until <see cref="IBinaryAsyncLockToken.Set"/> is called and then will get <see langword="true" />,
    /// or until until <see cref="IBinaryAsyncLockToken.Reset"/> is called and next awaiting caller will get <see langword="false" />,
    /// </remarks>
    public class BinaryAsyncLock {
        private static readonly Task<IBinaryAsyncLockToken> CompletedTask = Task.FromResult<IBinaryAsyncLockToken>(new Token());
        private TokenSource _tail;

        public bool IsSet => _tail != null && _tail.IsSet;

        public BinaryAsyncLock(bool isSet = false) {
            _tail = isSet ? new TokenSource(CompletedTask) : null;
        }

        public Task<IBinaryAsyncLockToken> WaitAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            while (true) {
                if (cancellationToken.IsCancellationRequested) {
                    return Task.FromCanceled<IBinaryAsyncLockToken>(cancellationToken);
                }

                var oldTail = _tail;
                if (oldTail != null && oldTail.IsSet) {
                    return oldTail.Task;
                }

                TokenSource newTail;
                if (oldTail == null) {
                    newTail = new TokenSource(this);
                } else {
                    newTail = new TokenSource();
                    if (oldTail.CompareExchangeNext(newTail, null) != null) {
                        // Another thread has provided a new tail
                        continue;
                    }
                }

                if (Interlocked.CompareExchange(ref _tail, newTail, oldTail) == oldTail) {
                    if (cancellationToken.CanBeCanceled) {
                        newTail.RegisterCancellation(cancellationToken);
                    }

                    return newTail.Task;
                }
            }
        }

        /// <summary>
        /// Creates a task that is completed when lock is in Unset state and no reset waiters are in front of current one
        /// and all tokens that were issued prior this one are released
        /// </summary>
        /// <remarks>
        /// Method tries to replace current tail with the new Reset TokenSource (when its task is completed, lock will be in Unset state),
        /// and if it fails, it means that another thread has updated the tail, so method tries again with that new tail.
        /// If there is no tail, it is considered that lock is in the Unset state already, but no one has requested a token yet
        /// If there is a tail, method tries to set its <see cref="TokenSource.Next"/> to the new TokenSource,
        /// and if it fails, it means that another thread has updated the property or the tail, so method tries again with that new tail.
        /// If replacing tail succeeded, method stops adding Set tokens. If there are no unreleased set tokens, new tail task is set to completed.
        /// </remarks>
        public Task<IBinaryAsyncLockToken> ResetAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            while (true) {
                if (cancellationToken.IsCancellationRequested) {
                    return Task.FromCanceled<IBinaryAsyncLockToken>(cancellationToken);
                }

                var oldTail = _tail;
                var newTail = oldTail == null || oldTail.IsSet ? new TokenSource(this) : new TokenSource(true);

                if (oldTail?.CompareExchangeNext(newTail, null) != null) {
                    // Another thread has provided a new tail
                    continue;
                }

                if (Interlocked.CompareExchange(ref _tail, newTail, oldTail) == oldTail) {
                    if (cancellationToken.CanBeCanceled) {
                        newTail.RegisterCancellation(cancellationToken);
                    }

                    return newTail.Task;
                }
            }
        }

        public void EnqueueReset() {
            ResetAsync().ContinueWith(t => {
                if (t.Status == TaskStatus.RanToCompletion) {
                    t.Result.Reset();
                }
            });
        }

        private void TokenSet(TokenSource tokenSource) {
            while (tokenSource != null) {
                Interlocked.CompareExchange(ref _tail, new TokenSource(CompletedTask), tokenSource);

                tokenSource = tokenSource.Next;
                if (tokenSource?.Tcs == null) {
                    return;
                }

                if (tokenSource.ResetOnSet && tokenSource.Tcs.TrySetResult(new Token(this, tokenSource))) {
                    return;
                }

                tokenSource.Tcs.TrySetResult(new Token());
            }
        }

        private void TokenReset(TokenSource tokenSource, bool setIfLast) {
            while (tokenSource != null) {
                var newTail = setIfLast ? new TokenSource(CompletedTask) : null;
                Interlocked.CompareExchange(ref _tail, newTail, tokenSource);

                tokenSource = tokenSource.Next;
                if (tokenSource?.Tcs == null) {
                    return;
                }

                // Try to reset tokenSource. If tokenSource.Tcs is canceled, try set result for the next one.
                if (tokenSource.Tcs.TrySetResult(new Token(this, tokenSource))) {
                    return;
                }
            }
        }

        private class Token : IBinaryAsyncLockToken {
            private readonly BinaryAsyncLock _binaryAsyncLock;
            private readonly TokenSource _tokenSource;

            public Token() {
                IsSet = true;
            }

            public Token(BinaryAsyncLock binaryAsyncLock, TokenSource tokenSource) {
                _binaryAsyncLock = binaryAsyncLock;
                _tokenSource = tokenSource;
                IsSet = false;
            }

            public bool IsSet { get; }
            public void Reset() => _binaryAsyncLock?.TokenReset(_tokenSource, false);
            public void Set() => _binaryAsyncLock?.TokenSet(_tokenSource);
            public void SetIfLast() => _binaryAsyncLock?.TokenReset(_tokenSource, true);
        }

        private class TokenSource {
            private TokenSource _next;

            public TaskCompletionSource<IBinaryAsyncLockToken> Tcs { get; }
            public Task<IBinaryAsyncLockToken> Task { get; }
            public TokenSource Next => _next;
            public bool ResetOnSet { get; }
            public bool IsSet => Task.Status == TaskStatus.RanToCompletion && Task.Result.IsSet;

            public TokenSource(BinaryAsyncLock binaryAsyncLock) {
                Task = System.Threading.Tasks.Task.FromResult<IBinaryAsyncLockToken>(new Token(binaryAsyncLock, this));
            }

            public TokenSource(Task<IBinaryAsyncLockToken> task) {
                Task = task;
            }

            public TokenSource(bool resetOnSet = false) {
                ResetOnSet = resetOnSet;
                Tcs = new TaskCompletionSource<IBinaryAsyncLockToken>();
                Task = Tcs.Task;
            }

            public TokenSource CompareExchangeNext(TokenSource value, TokenSource comparand)
                => Interlocked.CompareExchange(ref _next, value, comparand);

            public void RegisterCancellation(CancellationToken cancellationToken) {
                cancellationToken.Register(CancelTcs, cancellationToken);
            }

            private void CancelTcs(object state) {
                CancellationToken ct = (CancellationToken) state;
                Tcs?.TrySetCanceled(ct);
            }
        }
    }

    public interface IBinaryAsyncLockToken {
        bool IsSet { get; }
        void Reset();
        void Set();
        void SetIfLast();
    }
}
