// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Xml.Linq;
using static VisualRust.ProjectSystem.FileSystemMirroring.MsBuild.XProjHelpers;

namespace VisualRust.ProjectSystem.FileSystemMirroring.MsBuild {
    public class XPropertyGroup : XElement {
        public XPropertyGroup(params object[] elements) : base(MsBuildNamespace + "PropertyGroup", elements) { }

        public XPropertyGroup(string condition, params object[] elements)
            : base(MsBuildNamespace + "PropertyGroup", Content(elements, Attr("Condition", condition))) { }

        public XPropertyGroup(string label, string condition, params object[] elements)
            : base(MsBuildNamespace + "PropertyGroup", Content(elements, Attr("Label", label), Attr("Condition", condition))) { }
    }
}