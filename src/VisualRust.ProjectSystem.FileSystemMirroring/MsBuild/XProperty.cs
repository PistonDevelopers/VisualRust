// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Xml.Linq;
using static VisualRust.ProjectSystem.FileSystemMirroring.MsBuild.XProjHelpers;

namespace VisualRust.ProjectSystem.FileSystemMirroring.MsBuild {
    public class XProperty : XElement {
        public XProperty(string name) : base(MsBuildNamespace + name) { }

        public XProperty(string name, string value) : base(MsBuildNamespace + name, new XText(value)) { }

        public XProperty(string name, string condition, string value) : base(MsBuildNamespace + name, Attr("Condition", condition), new XText(value)) { }
    }
}