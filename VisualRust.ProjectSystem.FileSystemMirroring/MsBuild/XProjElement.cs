// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Xml.Linq;
using static VisualRust.ProjectSystem.FileSystemMirroring.MsBuild.XProjHelpers;

namespace VisualRust.ProjectSystem.FileSystemMirroring.MsBuild {
    public class XProjElement : XElement {
        public XProjElement(string name) : base(MsBuildNamespace + name) { }

        public XProjElement(string name, object content) : base(MsBuildNamespace + name, content) { }

        public XProjElement(string name, params object[] content) : base(MsBuildNamespace + name, content) { }
    }
}