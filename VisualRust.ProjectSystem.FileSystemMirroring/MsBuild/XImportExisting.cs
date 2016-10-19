// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using static System.FormattableString;

namespace VisualRust.ProjectSystem.FileSystemMirroring.MsBuild {
    public class XImportExisting : XImport {

        public XImportExisting(string project) : base(project, Invariant($"Exists('{project}')")) { }

        public XImportExisting(string project, string additionalCondition) : base(project, Invariant($"Exists('{project}') And {additionalCondition}")) { }
    }
}