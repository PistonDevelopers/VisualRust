// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.Common.Core.Disposables {
    /// <summary>
	/// Provides a set of static methods for creating Disposables. 
	/// </summary>
	public static class Disposable {
        /// <summary>
        /// Creates a disposable object that invokes the specified action when disposed.
        /// </summary>
        /// <param name="dispose">Action to run during the first call to <see cref="M:System.IDisposable.Dispose" />. The action is guaranteed to be run at most once.</param>
        /// <returns>The disposable object that runs the given action upon disposal.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="dispose" /> is null.</exception>
        public static IDisposable Create(Action dispose) {
            if (dispose == null) {
                throw new ArgumentNullException(nameof(dispose));
            }

            return new AnonymousDisposable(dispose);
        }

        /// <summary>
        /// Gets the disposable that does nothing when disposed.
        /// </summary>
        public static IDisposable Empty => DefaultDisposable.Instance;

        /// <summary>
        /// Represents an Action-based disposable.
        /// </summary>
        private sealed class AnonymousDisposable : IDisposable {
            private Action _dispose;

            /// <summary>
            /// Constructs a new disposable with the given action used for disposal.
            /// </summary>
            /// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
            public AnonymousDisposable(Action dispose) {
                _dispose = dispose;
            }

            /// <summary>
            /// Calls the disposal action if and only if the current instance hasn't been disposed yet.
            /// </summary>
            public void Dispose() {
                Action action = Interlocked.Exchange(ref _dispose, null);
                action?.Invoke();
            }
        }
    }
}
