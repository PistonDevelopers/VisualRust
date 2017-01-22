using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Core
{
    public interface IFileSystemWatcherFactory
    {
        FileSystemWatcherBase Create(string directory, string filter);
    }
}
