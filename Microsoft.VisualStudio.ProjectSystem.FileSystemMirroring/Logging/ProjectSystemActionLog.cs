// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Logging;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Logging {
    internal sealed class ProjectSystemActionLog : LinesLog {
        private static readonly Lazy<ProjectSystemActionLog> Instance = new Lazy<ProjectSystemActionLog>(
            () => new ProjectSystemActionLog(FileLogWriter.InTempFolder("Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring")));

        public static IActionLog Default => Instance.Value;

        private ProjectSystemActionLog(IActionLogWriter logWriter) :
            base(logWriter) {
        }
    }
}
