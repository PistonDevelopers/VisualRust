// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using static System.FormattableString;

namespace VisualRust.ProjectSystem.FileSystemMirroring.MsBuild {
    public class XDefaultValueProperty : XProperty {
        public XDefaultValueProperty(string name, string defaultValue)
            : base(name, Invariant($"'$({name})' == ''"), defaultValue) { }
    }
}