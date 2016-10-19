// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace VisualRust.ProjectSystem.FileSystemMirroring.Utilities {
    public static class PathExtensions {
        /// <summary>
        /// Converts path to file to a relative 8.3 path. Returns null if file 
        /// does not exist since OS can only perform 8.3 conversion to existing files.
        /// </summary>
        public static string ToShortRelativePath(this IFileSystem fileSystem, string path, string rootFolder) {
            // ToShortPath will return null if files does not exist since conversion 
            // to 8.3 path can only be performed to files that exist.
            var shortPath = fileSystem.ToShortPath(path); 
            var rootShortPath = fileSystem.ToShortPath(rootFolder);
            return !string.IsNullOrEmpty(shortPath) ? PathHelper.MakeRelative(rootShortPath, shortPath) : null;
        }
    }
}
