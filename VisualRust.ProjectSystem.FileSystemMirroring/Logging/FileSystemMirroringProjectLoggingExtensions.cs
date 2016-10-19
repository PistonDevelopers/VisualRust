// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Build.Construction;
using Microsoft.Common.Core.Logging;

namespace VisualRust.ProjectSystem.FileSystemMirroring.Logging {
    internal static class FileSystemMirroringProjectLoggingExtensions {
        public static void ApplyProjectChangesStarted(this IActionLog log) {
            log.WriteLineAsync(LogVerbosity.Normal, MessageCategory.General, "Starting applying changes to file-mirroring project");
        }

        public static void ApplyProjectChangesFinished(this IActionLog log) {
            log.WriteLineAsync(LogVerbosity.Normal, MessageCategory.General, "Finished applying changes to file-mirroring project");
        }

        public static void MsBuildAfterChangesApplied(this IActionLog log, ProjectRootElement rootElement) {
            log.WriteLineAsync(LogVerbosity.Normal, MessageCategory.General, "File mirroring project after changes applied:" + Environment.NewLine + rootElement.RawXml);
        }
    }
}