// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild {
    public class XProjDocument : XDocument {
        public XProjDocument()
            : base(new XDeclaration("1.0", "utf-8", null)) { }

        public XProjDocument(XProject xProject)
            : base(new XDeclaration("1.0", "utf-8", null), xProject) { }

        public string GetFullXml() {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb, CultureInfo.InvariantCulture)) {
                Save(writer);
            }
            return sb.ToString();
        }
    }
}
