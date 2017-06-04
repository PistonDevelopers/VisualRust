// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Build.Construction;
using VisualRust.Core;

namespace VisualRust.ProjectSystem.FileSystemMirroring.Logging {
    internal static class FileSystemMirroringProjectLoggingExtensions {
        public static void ApplyProjectChangesStarted(this IActionLog log) {
            log.Trace("Starting applying changes to file-mirroring project");
        }

        public static void ApplyProjectChangesFinished(this IActionLog log) {
            log.Trace("Finished applying changes to file-mirroring project");
        }

        public static void MsBuildAfterChangesApplied(this IActionLog log, ProjectRootElement rootElement) {
            log.Trace("File mirroring project after changes applied:" + Environment.NewLine + rootElement.RawXml);
        }
    }
}