using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;

namespace VisualRust.ProjectSystem
{
    internal sealed class MsBuildFileSystemFilter : IMsBuildFileSystemFilter
    {
        public bool IsFileAllowed(string relativePath, FileAttributes attributes)
        {
            return !attributes.HasFlag(FileAttributes.Hidden)
                && !HasExtension(relativePath, ".user", ".rsproj", ".sln");
        }

        public bool IsDirectoryAllowed(string relativePath, FileAttributes attributes)
        {
            return !attributes.HasFlag(FileAttributes.Hidden) && !relativePath.StartsWith("target\\");
        }

        public void Seal() { }

        private static bool HasExtension(string filePath, params string[] possibleExtensions)
        {
            var extension = Path.GetExtension(filePath);
            return !string.IsNullOrEmpty(extension) && possibleExtensions.Any(pe => extension.EqualsIgnoreCase(pe));
        }
    }
}
