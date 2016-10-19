// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Shell {
    public interface IProgressDialog {
        /// <summary>
        /// Shows progress bar that blocks hosting shell
        /// </summary>
        /// <returns></returns>
        void Show(Func<CancellationToken, Task> method, string waitMessage, int delayToShowDialogMs = 0);

        /// <summary>
        /// Shows progress bar that blocks hosting shell
        /// </summary>
        /// <returns></returns>
        TResult Show<TResult>(Func<CancellationToken, Task<TResult>> method, string waitMessage, int delayToShowDialogMs = 0);

        /// <summary>
        /// Shows progress bar that blocks hosting shell
        /// </summary>
        /// <returns></returns>
        void Show(Func<IProgress<ProgressDialogData>, CancellationToken, Task> method, string waitMessage, int totalSteps = 100, int delayToShowDialogMs = 0);

        /// <summary>
        /// Shows progress bar that blocks hosting shell
        /// </summary>
        /// <returns></returns>
        T Show<T>(Func<IProgress<ProgressDialogData>, CancellationToken, Task<T>> method, string waitMessage, int totalSteps = 100, int delayToShowDialogMs = 0);
    }
}