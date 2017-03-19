using System;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Text;
using VisualRust.Core;
using BaseFileSystem = System.IO.Abstractions.FileSystem;

namespace VisualRust.ProjectSystem
{
    class FileSystem : BaseFileSystem, Core.IFileSystem
    {
        class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern uint GetShortPathName(
                [MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath,
                [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath,
                uint cchBuffer);
        }

        class FileSystemWatcherFactory : IFileSystemWatcherFactory
        {
            public FileSystemWatcherBase Create(string directory, string filter)
            {
                return new FileSystemWatcherWrapper(directory, filter);
            }
        }

        readonly IFileSystemWatcherFactory fsWatcherFactory = new FileSystemWatcherFactory();

        public IFileSystemWatcherFactory FileSystemWatcher { get { return fsWatcherFactory; } }

        public string ToShortPath(string path)
        {
            uint shortLength = NativeMethods.GetShortPathName(path, null, 0);
            if(shortLength == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            var sb = new StringBuilder((int)shortLength - 1);
            if(NativeMethods.GetShortPathName(path, sb, shortLength) == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return sb.ToString();
        }
    }
}