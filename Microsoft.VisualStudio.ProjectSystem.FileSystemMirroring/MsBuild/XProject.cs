// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Xml.Linq;
using static Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild.XProjHelpers;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild {
    public class XProject : XElement {
        public XProject() : base(MsBuildNamespace + "Project") { }

        public XProject(string toolsVersion = null, string defaultTargets = null, params object[] elements)
            : base(MsBuildNamespace + "Project", Content(elements, Attr("ToolsVersion", toolsVersion), Attr("DefaultTargets", defaultTargets))) { }
    }
}