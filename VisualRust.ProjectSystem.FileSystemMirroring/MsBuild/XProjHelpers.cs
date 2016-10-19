// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Xml.Linq;

namespace VisualRust.ProjectSystem.FileSystemMirroring.MsBuild {
    internal class XProjHelpers {
        public static XNamespace MsBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public static XAttribute Attr(string name, object value) {
            return value != null ? new XAttribute(name, value) : null;
        }

        public static object[] Content(object[] elements, params XAttribute[] attributes) {
            return attributes
                .Where(a => a != null)
                .Concat(elements).ToArray();
        }
    }
}