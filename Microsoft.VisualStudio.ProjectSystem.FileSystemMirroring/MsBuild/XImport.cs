// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Xml.Linq;
using static Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild.XProjHelpers;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild {
    public class XImport : XElement {
        public XImport(string project) : base(MsBuildNamespace + "Import", Attr("Project", project)) { }

        public XImport(string project, string condition) : base(MsBuildNamespace + "Import", Attr("Project", project), Attr("Condition", condition)) { }
    }
}