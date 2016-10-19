// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.Shell {
    public struct ProgressDialogData {
        public int Step { get; }
        public string ProgressText { get; }
        public string StatusBarText { get; }
        public string WaitMessage { get; }

        public ProgressDialogData(int step, string progressText = null, string statusBarText = null, string waitMessage = null) {
            Step = step;
            ProgressText = progressText;
            StatusBarText = statusBarText;
            WaitMessage = waitMessage;
        }
    }
}