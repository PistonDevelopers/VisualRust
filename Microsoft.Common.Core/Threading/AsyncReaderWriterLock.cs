using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Threading {
    /// <summary>
    /// Async lock-free version of ReaderWriterLock with cancellation and reentrancy support. Upgrades aren't supported
    /// </summary>
    /// <remarks>
    /// <para>
    /// This lock prefers writers over readers. If writer lock is requested, all furhter reader locks will be provided only after writer lock is released.
    /// For example, if reader holds a lock and several locks are requested in order Writer-Reader-Writer, they will be provider as Writer-Writer-Reader 
    /// after current lock is released
    /// </para>
    /// <para>
    /// Cancellation affects only unfulfilled lock requests. Canceled requests affect the waiting queue also prefering writers over readers.
    /// For example, if reader holds a lock and several locks are requested in order Writer-Reader-Writer, the waiting queue will be Writer-Writer-Reader,
    /// sp cancelling first writer request will not fulfill following reader request, but instead will put the second waiter in front of the queue.
    /// </para>
    /// <para>
    /// async/await doesn't support reentrancy on the language level, so to support it <see cref="AsyncReaderWriterLock"/> uses <see cref="ReentrancyToken"/> structure.
    /// It can be passed as a method argument in a way similar to the <see cref="CancellationToken"/>.
    /// There are 4 possible types of reentrancy:
    /// <list type="bullet">
    ///    <item>
    ///        <term>Reader requested inside Reader lock</term>
    ///        <description>Reader locks don't block each other, so it will be treated as another reader lock request. The difference is that it will have priority over writer.</description>
    ///    </item>
    ///    <item>
    ///        <term>Reader requested inside Writer lock</term>
    ///        <description>Lock will increment reentrancy counter of the Writer lock, so it will require them both to be released before next lock can be provided</description>
    ///    </item>
    ///    <item>
    ///        <term>Writer requested inside Writer lock</term>
    ///        <description>Lock will increment reentrancy counter of the Writer lock, so it will require them both to be released before next lock can be provided</description>
    ///    </item>
    ///    <item>
    ///        <term>Writer requested inside reader lock</term>
    ///        <description>That is considered an upgrade, which aren't supported right now, so in this case request will be treated as non-reentrant</description>
    ///    </item>
    ///</list>
    /// </para>
    /// </remarks>
    public class AsyncReaderWriterLock {
        private ReaderLockSource _readerTail;
        private WriterLockSource _writerTail;
        private WriterLockSource _lastAcquiredWriter;

        private static readonly IReentrancyTokenFactory<ReaderLockSource> ReaderLockTokenFactory;
        private static readonly IReentrancyTokenFactory<WriterLockSource> WriterLockTokenFactory;

        static AsyncReaderWriterLock() {
            ReaderLockTokenFactory = ReentrancyToken.CreateFactory<ReaderLockSource>();
            WriterLockTokenFactory = ReentrancyToken.CreateFactory<WriterLockSource>();
        }

        public AsyncReaderWriterLock() {
            _readerTail = new ReaderLockSource(this);
            _writerTail = new WriterLockSource(this);

            _readerTail.CompleteTasks();
            _writerTail.TryCompleteTask();
            _writerTail.Task.Result.Dispose();
        }

        public Task<IAsyncReaderWriterLockToken> ReaderLockAsync(CancellationToken cancellationToken = default(CancellationToken), ReentrancyToken reentrancyToken = default(ReentrancyToken)) {
            Task<IAsyncReaderWriterLockToken> task;
            var writerFromToken = WriterLockTokenFactory.GetSource(reentrancyToken);
            if (writerFromToken != null && writerFromToken.TryReenter(out task)) {
                return task;
            }

            var readerFromToken = ReaderLockTokenFactory.GetSource(reentrancyToken);
            if (readerFromToken != null && readerFromToken.TryReenter(out task)) {
                return task;
            }

            while (true) {
                if (cancellationToken.IsCancellationRequested) {
                    return Task.FromCanceled<IAsyncReaderWriterLockToken>(cancellationToken);
                }
            
                task = _readerTail.WaitAsync(cancellationToken);
                if (task != null) {
                    return task;
                }
            }
        }

        public Task<IAsyncReaderWriterLockToken> WriterLockAsync(CancellationToken cancellationToken = default(CancellationToken), ReentrancyToken reentrancyToken = default(ReentrancyToken)) {
            Task<IAsyncReaderWriterLockToken> task;
            var writerFromToken = WriterLockTokenFactory.GetSource(reentrancyToken);
            if (writerFromToken != null && writerFromToken.TryReenter(out task)) {
                return task;
            }

            while (true) {
                if (cancellationToken.IsCancellationRequested) {
                    return Task.FromCanceled<IAsyncReaderWriterLockToken>(cancellationToken);
                }

                WriterLockSource oldWriter = _writerTail;
                WriterLockSource newWriter;
                if (!oldWriter.TrySetNext(out newWriter)) {
                    continue;
                }

                var reader = oldWriter.IsCompleted && _lastAcquiredWriter == oldWriter
                    ? Interlocked.Exchange(ref _readerTail, new ReaderLockSource(this)) 
                    : null;

                if (Interlocked.CompareExchange(ref _writerTail, newWriter, oldWriter) != oldWriter) {
                    throw new InvalidOperationException();
                }

                if (reader != null) { // oldWriter.IsCompleted
                    reader.RegisterWait(newWriter, cancellationToken);
                } else {
                    oldWriter.RegisterWait(cancellationToken);
                }

                return newWriter.Task;
            }
        }

        private void CompleteNextTask(WriterLockSource writer) {
            var reader = _readerTail;
            while (writer != null && !writer.TryCompleteTask()) {
                writer = writer.NextWriter;
            }

            // No writer source left - complete reader source tasks
            if (writer == null) {
                reader.CompleteTasks();
            }
        }

        private void CompleteReaderTasksIfRequired(WriterLockSource writer) {
            var reader = _readerTail;
            while (writer != null && writer.IsCompleted) {
                writer = writer.NextWriter;
            }

            // No writer source left - complete reader source tasks
            if (writer == null) {
                reader.CompleteTasks();
            }
        }

        private class ReaderLockSource {
            private readonly AsyncReaderWriterLock _host;
            private readonly TaskCompletionSource<bool> _rootTcs;
            private WriterLockSource _writer;
            private int _count;

            public ReaderLockSource(AsyncReaderWriterLock host) {
                _host = host;
                _rootTcs = new TaskCompletionSource<bool>();
            }

            public Task<IAsyncReaderWriterLockToken> WaitAsync(CancellationToken cancellationToken) {
                if (!TryIncrement()) {
                    return null;
                }

                if (_rootTcs.Task.Status == TaskStatus.RanToCompletion) {
                    return Task.FromResult<IAsyncReaderWriterLockToken>(new Token(Decrement, this));
                }

                var tcs = new TaskCompletionSource<IAsyncReaderWriterLockToken>();

                if (cancellationToken.CanBeCanceled) {
                    cancellationToken.Register(Cancellation, tcs, false);
                }

                _rootTcs.Task.ContinueWith(Continuation, tcs, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                return tcs.Task;
            }

            public bool TryReenter(out Task<IAsyncReaderWriterLockToken> task) {
                while (true) {
                    var count = _count;
                    if (count == 0) {
                        task = null;
                        return false;
                    }

                    if (Interlocked.CompareExchange(ref _count, count + 1, count) == count) {
                        IAsyncReaderWriterLockToken token = new Token(Decrement, this);
                        task = Task.FromResult(token);
                        return true;
                    }
                }
            }

            private void Continuation(Task<bool> task, object state)
                => ((TaskCompletionSource<IAsyncReaderWriterLockToken>)state).TrySetResult(new Token(Decrement, this));

            private void Cancellation(object state) {
                if (((TaskCompletionSource<IAsyncReaderWriterLockToken>) state).TrySetCanceled()) {
                    Decrement();
                }
            }

            public void CompleteTasks() => _rootTcs.TrySetResult(true);

            public void RegisterWait(WriterLockSource writer, CancellationToken cancellationToken) {
                Interlocked.Exchange(ref _writer, writer);

                CompleteTasks();

                if (cancellationToken.IsCancellationRequested) {
                    CancelWaitReaders(cancellationToken);
                } else if (_count == 0) {
                    writer.TryCompleteTask();
                } else if (cancellationToken.CanBeCanceled) {
                    cancellationToken.Register(CancelWaitReaders, cancellationToken, false);
                }
            }

            private void CancelWaitReaders(object state) {
                if (!_writer.TryCancelTask((CancellationToken) state)) {
                    return;
                }

                _host.CompleteReaderTasksIfRequired(_writer.NextWriter);
            }

            private bool TryIncrement() {
                // _writer != null means that current ReaderLockSource can't create any more readers 
                // see WriterLockAsync
                if (_writer != null) {
                    return false;
                }

                Interlocked.Increment(ref _count);
                // If _writer != null became not null when _count was incremented, Decrement it back
                if (_writer != null) {
                    Decrement();
                    return false;
                }

                return true;
            }

            private void Decrement() {
                var count = Interlocked.Decrement(ref _count);

                if (count == 0) {
                    _host.CompleteNextTask(_writer);
                }
            }
        }

        private class WriterLockSource {
            private readonly TaskCompletionSource<IAsyncReaderWriterLockToken> _tcs;
            private readonly AsyncReaderWriterLock _host;
            private WriterLockSource _nextWriter;
            private int _reentrancyCount;

            public Task<IAsyncReaderWriterLockToken> Task => _tcs.Task;
            public WriterLockSource NextWriter => _nextWriter;
            public bool IsCompleted => _tcs.Task.IsCanceled || _reentrancyCount == 0;

            public WriterLockSource(AsyncReaderWriterLock host) {
                _reentrancyCount = 1;
                _host = host;
                _tcs = new TaskCompletionSource<IAsyncReaderWriterLockToken>();
            }

            public bool TrySetNext(out WriterLockSource next) {
                if (_nextWriter != null) {
                    next = null;
                    return false;
                }

                next = new WriterLockSource(_host);
                return Interlocked.CompareExchange(ref _nextWriter, next, null) == null;
            }

            public bool TryReenter(out Task<IAsyncReaderWriterLockToken> task) {
                while (true) {
                    var count = _reentrancyCount;
                    if (count == 0) {
                        task = null;
                        return false;
                    }

                    if (Interlocked.CompareExchange(ref _reentrancyCount, count + 1, count) == count) {
                        IAsyncReaderWriterLockToken token = new Token(DecrementReentrancy, this);
                        task = System.Threading.Tasks.Task.FromResult(token);
                        return true;
                    }
                }
            }

            public bool TryCompleteTask() {
                var isSet = _tcs.TrySetResult(new Token(DecrementReentrancy, this));
                if (isSet) {
                    Interlocked.Exchange(ref _host._lastAcquiredWriter, this);
                }
                return isSet;
            }

            public bool TryCancelTask(CancellationToken cancellationToken) => _tcs.TrySetCanceled(cancellationToken);

            public void RegisterWait(CancellationToken cancellationToken) {
                if (cancellationToken.IsCancellationRequested) {
                    _nextWriter.TryCancelTask(cancellationToken);
                }

                if (cancellationToken.CanBeCanceled) {
                    cancellationToken.Register(CancelWait, cancellationToken, false);
                }
            }

            private void DecrementReentrancy() {
                if (Interlocked.Decrement(ref _reentrancyCount) == 0) {
                    _host.CompleteNextTask(this);
                }
            }

            private void CancelWait(object state) {
                if (_nextWriter.TryCancelTask((CancellationToken) state)) {
                    var writer = _host._lastAcquiredWriter;
                    _host.CompleteReaderTasksIfRequired(writer);
                }
            }
        }

        private class Token : IAsyncReaderWriterLockToken {
            private Action _dispose;
            public ReentrancyToken Reentrancy { get; }

            public Token(Action dispose, WriterLockSource reentrancySource) {
                _dispose = dispose;
                Reentrancy = WriterLockTokenFactory.Create(reentrancySource);
            }

            public Token(Action dispose, ReaderLockSource reentrancySource) {
                _dispose = dispose;
                Reentrancy = ReaderLockTokenFactory.Create(reentrancySource);
            }

            public void Dispose() {
                var dispose = Interlocked.Exchange(ref _dispose, null);
                dispose?.Invoke();
            }
        }
    }
}