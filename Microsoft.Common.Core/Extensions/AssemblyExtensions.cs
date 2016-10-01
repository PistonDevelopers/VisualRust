// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.Common.Core {
    public static class AssemblyExtensions {
        public static string GetAssemblyPath(this Assembly assembly) {
            var codeBase = assembly.CodeBase;
            return new Uri(codeBase).LocalPath;
        }
    }
}