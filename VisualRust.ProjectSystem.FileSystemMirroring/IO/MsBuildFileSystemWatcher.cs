// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft;
using VisualRust.ProjectSystem.FileSystemMirroring.Logging;
using VisualRust.ProjectSystem.FileSystemMirroring.Utilities;
using System.IO.Abstractions;
using NotifyFilters = System.IO.NotifyFilters;
using IOException = System.IO.IOException;
using ErrorEventArgs = System.IO.ErrorEventArgs;

#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif
using static System.FormattableString;

namespace VisualRust.ProjectSystem.FileSystemMirroring.IO {
    using Microsoft.Common.Core;
    using VisualRust.Core;

    public sealed partial class MsBuildFileSystemWatcher : IDisposable {
        private readonly string _directory;
        private readonly string _filter;
        private readonly MsBuildFileSystemWatcherEntries _entries;
        private readonly ConcurrentQueue<IFileSystemChange> _queue;
        private readonly int _delayMilliseconds;
        private readonly int _recoveryDelayMilliseconds;
        private readonly IFileSystem _fileSystem;
        private readonly IMsBuildFileSystemFilter _fileSystemFilter;
        private readonly TaskScheduler _taskScheduler;
        private readonly BroadcastBlock<Changeset> _broadcastBlock;
        private readonly IActionLog _log;
        private FileSystemWatcherBase _fileWatcher;
        private FileSystemWatcherBase _directoryWatcher;
        private FileSystemWatcherBase _attributesWatcher;
        private int _consumerIsWorking;
        private int _hasErrors;

        public IReceivableSourceBlock<Changeset> SourceBlock { get; }
        public event EventHandler<EventArgs> Error;

        public MsBuildFileSystemWatcher(string directory, string filter, int delayMilliseconds, int recoveryDelayMilliseconds, IFileSystem fileSystem, IMsBuildFileSystemFilter fileSystemFilter, IActionLog log, TaskScheduler taskScheduler = null) {
            Requires.NotNullOrWhiteSpace(directory, nameof(directory));
            Requires.NotNullOrWhiteSpace(filter, nameof(filter));
            Requires.Range(delayMilliseconds >= 0, nameof(delayMilliseconds));
            Requires.Range(recoveryDelayMilliseconds >= 0, nameof(recoveryDelayMilliseconds));
            Requires.NotNull(fileSystem, nameof(fileSystem));
            Requires.NotNull(fileSystemFilter, nameof(fileSystemFilter));

            _directory = directory;
            _filter = filter;
            _delayMilliseconds = delayMilliseconds;
            _recoveryDelayMilliseconds = recoveryDelayMilliseconds;
            _fileSystem = fileSystem;
            _fileSystemFilter = fileSystemFilter;
            _taskScheduler = taskScheduler ?? TaskScheduler.Default;
            _log = log;

            _entries = new MsBuildFileSystemWatcherEntries();
            _queue = new ConcurrentQueue<IFileSystemChange>();
            _broadcastBlock = new BroadcastBlock<Changeset>(b => b, new DataflowBlockOptions { TaskScheduler = _taskScheduler });
            SourceBlock = _broadcastBlock.SafePublicize();
            _fileSystemFilter.Seal();
        }

        public void Dispose() {
            _fileWatcher?.Dispose();
            _directoryWatcher?.Dispose();
            _attributesWatcher?.Dispose();
        }

        public void Start() {
            _log.WatcherStarting();

            Enqueue(new DirectoryCreated(_entries, _directory, _fileSystem, _fileSystemFilter, _directory));

            _fileWatcher = CreateFileSystemWatcher(NotifyFilters.FileName);
            _fileWatcher.Created += (sender, e) => Enqueue(new FileCreated(_entries, _directory, _fileSystem, _fileSystemFilter, e.FullPath));
            _fileWatcher.Deleted += (sender, e) => Enqueue(new FileDeleted(_entries, _directory, e.FullPath));
            _fileWatcher.Renamed += (sender, e) => Enqueue(new FileRenamed(_entries, _directory, _fileSystem, _fileSystemFilter, e.OldFullPath, e.FullPath));
            _fileWatcher.Error += (sender, e) => FileSystemWatcherError("File Watcher", e);

            _directoryWatcher = CreateFileSystemWatcher(NotifyFilters.DirectoryName);
            _directoryWatcher.Created += (sender, e) => Enqueue(new DirectoryCreated(_entries, _directory, _fileSystem, _fileSystemFilter, e.FullPath));
            _directoryWatcher.Deleted += (sender, e) => Enqueue(new DirectoryDeleted(_entries, _directory, e.FullPath));
            _directoryWatcher.Renamed += (sender, e) => Enqueue(new DirectoryRenamed(_entries, _directory, _fileSystem, _fileSystemFilter, e.OldFullPath, e.FullPath));
            _directoryWatcher.Error += (sender, e) => FileSystemWatcherError("Directory Watcher", e);

            _attributesWatcher = CreateFileSystemWatcher(NotifyFilters.Attributes);
            _attributesWatcher.Changed += (sender, e) => Enqueue(new AttributesChanged(_entries, e.Name, e.FullPath));
            _attributesWatcher.Error += (sender, e) => FileSystemWatcherError("Attributes Watcher", e);

            _log.WatcherStarted();
        }

        private void Enqueue(IFileSystemChange change) {
            _queue.Enqueue(change);
            StartConsumer();
        }

        private void StartConsumer() {
            if (Interlocked.Exchange(ref _consumerIsWorking, 1) == 0) {
                Task.Factory
                    .StartNew(async () => await ConsumeWaitPublish(), CancellationToken.None, Task.Factory.CreationOptions, _taskScheduler)
                    .Unwrap();
                _log.WatcherConsumeChangesScheduled();
            }
        }

        private async Task ConsumeWaitPublish() {
            _log.WatcherConsumeChangesStarted();

            try {
                while (!_queue.IsEmpty || _hasErrors != 0) {
                    Consume();
                    await Task.Delay(_delayMilliseconds);
                    if (Interlocked.Exchange(ref _hasErrors, 0) == 0) {
                        continue;
                    }

                    var isRecovered = await TryRecover();
                    if (!isRecovered) {
                        return;
                    }
                }

                var changeset = _entries.ProduceChangeset();
                if (!changeset.IsEmpty()) {
                    _broadcastBlock.Post(changeset);
                    _log.WatcherChangesetSent(changeset);
                }
            } finally {
                _consumerIsWorking = 0;
                _log.WatcherConsumeChangesFinished();
                if (!_queue.IsEmpty) {
                    StartConsumer();
                }
            }
        }

        private void Consume() {
            IFileSystemChange change;
            while (_hasErrors == 0 && _queue.TryDequeue(out change)) {
                try {
                    _log.WatcherApplyChange(change.ToString());
                    change.Apply();
                    if (_entries.RescanRequired) {
                        _hasErrors = 1;
                    }
                } catch (Exception e) {
                    _hasErrors = 1;
                    _log.WatcherApplyChangeFailed(change.ToString(), e);
                    System.Diagnostics.Debug.Fail(Invariant($"Failed to apply change {change}:\n{e}"));
                }
            }
        }

        private async Task<bool> TryRecover() {
            _fileWatcher.EnableRaisingEvents = false;
            _directoryWatcher.EnableRaisingEvents = false;
            _attributesWatcher.EnableRaisingEvents = false;

            await Task.Delay(_recoveryDelayMilliseconds);
            EmptyQueue();

            var rescanChange = new DirectoryCreated(_entries, _directory, _fileSystem, _fileSystemFilter, _directory);
            bool isRecovered;
            try {
                _entries.MarkAllDeleted();
                _log.WatcherApplyRecoveryChange(rescanChange.ToString());
                rescanChange.Apply();
                isRecovered = !_entries.RescanRequired;
            } catch (Exception e) {
                _log.WatcherApplyRecoveryChangeFailed(rescanChange.ToString(), e);
                isRecovered = false;
            }

            if (isRecovered) {
                try {
                    _fileWatcher.EnableRaisingEvents = true;
                    _directoryWatcher.EnableRaisingEvents = true;
                    _attributesWatcher.EnableRaisingEvents = true;
                } catch (Exception) {
                    isRecovered = false;
                }
            }

            if (!isRecovered) {
                Error?.Invoke(this, new EventArgs());
                Dispose();
            }

            return isRecovered;
        }

        private void EmptyQueue() {
            IFileSystemChange change;
            while (_queue.TryDequeue(out change)) { }
        }

        private FileSystemWatcherBase CreateFileSystemWatcher(NotifyFilters notifyFilter) {
            var watcher = _fileSystem.FileSystemWatcher.Create(_directory, _filter);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.InternalBufferSize = 65536;
            watcher.NotifyFilter = notifyFilter;
            return watcher;
        }

        private static bool IsFileAllowed(string rootDirectory, string fullPath,
            IFileSystem fileSystem, IMsBuildFileSystemFilter filter,
            out string relativePath, out string shortRelativePath) {
            relativePath = null;
            shortRelativePath = null;
            if (fullPath.StartsWithIgnoreCase(rootDirectory)) {
                relativePath = PathHelper.MakeRelative(rootDirectory, fullPath);
                 try {
                    shortRelativePath = fileSystem.ToShortRelativePath(fullPath, rootDirectory);
                    return !string.IsNullOrEmpty(shortRelativePath) && filter.IsFileAllowed(relativePath, fileSystem.FileInfo.FromFileName(fullPath).Attributes);
                } catch (IOException) { } catch (UnauthorizedAccessException) { } // File isn't allowed if it isn't accessible
            }
            return false;
        }

        private interface IFileSystemChange {
            void Apply();
        }

        private void FileSystemWatcherError(string watcherName, ErrorEventArgs errorEventArgs) {
            _log.ErrorInFileSystemWatcher(watcherName, errorEventArgs.GetException());
            Interlocked.Exchange(ref _hasErrors, 1);
            StartConsumer();
        }

        public class Changeset {
            public HashSet<string> AddedFiles { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> AddedDirectories { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> RemovedFiles { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> RemovedDirectories { get; } = new HashSet<string>(StringComparer.Ordinal);
            public Dictionary<string, string> RenamedFiles { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
            public Dictionary<string, string> RenamedDirectories { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

            public bool IsEmpty() {
                return AddedFiles.Count == 0
                    && AddedDirectories.Count == 0
                    && RemovedFiles.Count == 0
                    && RemovedDirectories.Count == 0
                    && RenamedFiles.Count == 0
                    && RenamedDirectories.Count == 0;
            }
        }
    }
}
