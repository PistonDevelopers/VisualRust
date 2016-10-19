// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities.Designers;
#endif
#if VS15
using Microsoft.VisualStudio.ProjectSystem;
#endif

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring {
    public static class ProjectTreeExtensions {
        public static bool IsProjectSelected(this IImmutableSet<IProjectTree> nodes) {
            return nodes != null && nodes.Count == 1 && nodes.First().Root == nodes.First();
        }

        public static string GetSingleNodePath(this IImmutableSet<IProjectTree> nodes) {
            if (nodes != null && nodes.Count == 1) {
                return nodes.First().FilePath;
            }
            return string.Empty;
        }

        public static bool IsSingleNodePath(this IImmutableSet<IProjectTree> nodes) {
            return !string.IsNullOrEmpty(GetSingleNodePath(nodes));
        }

        public static bool IsFolder(this IImmutableSet<IProjectTree> nodes) {
            if (nodes != null && nodes.Count == 1) {
                return nodes.First().IsFolder;
            }
            return false;
        }

        public static string GetNodeFolderPath(this IImmutableSet<IProjectTree> nodes) {
            var path = nodes.GetSingleNodePath();
            if (!string.IsNullOrEmpty(path)) {
                if (Directory.Exists(path)) {
                    return path;
                } else if (File.Exists(path)) {
                    return Path.GetDirectoryName(path);
                }
            }
            return string.Empty;
        }

        public static IEnumerable<string> GetSelectedNodesPaths(this IImmutableSet<IProjectTree> nodes) {
            if (nodes != null && nodes.Count > 0) {
                return nodes.Where(x => !string.IsNullOrEmpty(x?.FilePath)).Select(x => x.FilePath);
            }
            return Enumerable.Empty<string>();
        }

        public static IEnumerable<string> GetSelectedFilesPaths(this IImmutableSet<IProjectTree> nodes) {
            if (nodes != null && nodes.Count > 0) {
                return nodes.Where(x => !x.IsFolder && !string.IsNullOrEmpty(x?.FilePath)).Select(x => x.FilePath);
            }
            return Enumerable.Empty<string>();
        }

        public static IEnumerable<string> GetAllFilePaths(this IEnumerable<IProjectTree> nodes) {
            List<string> paths = new List<string>();
            foreach (IProjectTree node in nodes) {
                if (node.IsFolder || node.Children.Count > 0) {
                    paths.AddRange(node.Children.GetAllFilePaths());
                } else if (!string.IsNullOrWhiteSpace(node.FilePath)) {
                    paths.Add(node.FilePath);
                }
            }
            return paths.Distinct();
        }

        public static string GetSelectedFolderPath(this IImmutableSet<IProjectTree> nodes, UnconfiguredProject unconfiguredProject) {
            if (nodes.Count == 1) {
                var n = nodes.First();
                if (n.IsRoot()) {
                    return Path.GetDirectoryName(unconfiguredProject.FullPath);
                }
                return nodes.GetNodeFolderPath();
            }
            return string.Empty;
        }
    }
}
