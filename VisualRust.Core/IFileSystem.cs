using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Core
{
    // Covering a blind spot in System.IO.Abstractions
    public interface IFileSystem : System.IO.Abstractions.IFileSystem
    {
        IFileSystemWatcherFactory FileSystemWatcher { get; }
        string ToShortPath(string path);
        string ToShortRelativePath(string path);
    }
}
