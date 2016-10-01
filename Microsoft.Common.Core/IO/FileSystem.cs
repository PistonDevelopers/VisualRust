// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Microsoft.Extensions.FileSystemGlobbing;

namespace Microsoft.Common.Core.IO {
    public sealed class FileSystem : IFileSystem {
        public IFileSystemWatcher CreateFileSystemWatcher(string path, string filter) => new FileSystemWatcherProxy(path, filter);
        
        public IDirectoryInfo GetDirectoryInfo(string directoryPath) => new DirectoryInfoProxy(directoryPath);
        
        public bool FileExists(string path) => File.Exists(path);

        public string ReadAllText(string path) => File.ReadAllText(path);
        
        public void WriteAllText(string path, string content) => File.WriteAllText(path, content);
        
        public IEnumerable<string> FileReadAllLines(string path) => File.ReadLines(path);
        
        public void FileWriteAllLines(string path, IEnumerable<string> contents) => File.WriteAllLines(path, contents);

        public byte[] FileReadAllBytes(string path) => File.ReadAllBytes(path);
        
        public void FileWriteAllBytes(string path, byte[] bytes) => File.WriteAllBytes(path, bytes);

        public Stream CreateFile(string path) => File.Create(path);
        public Stream FileOpen(string path, FileMode mode) => File.Open(path, mode);

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public FileAttributes GetFileAttributes(string path) => File.GetAttributes(path);
        
        public string ToLongPath(string path) {
            var sb = new StringBuilder(NativeMethods.MAX_PATH);
            NativeMethods.GetLongPathName(path, sb, sb.Capacity);
            return sb.ToString();
        }
         
        public string ToShortPath(string path) {
            var sb = new StringBuilder(NativeMethods.MAX_PATH);
            NativeMethods.GetShortPathName(path, sb, sb.Capacity);
            return sb.ToString();
        }

        public IFileVersionInfo GetVersionInfo(string path) {
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
            return new FileVersionInfo(fvi.FileMajorPart, fvi.FileMinorPart);
        }

        public void DeleteFile(string path) => File.Delete(path);

        public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);

        public string[] GetFileSystemEntries(string path, string searchPattern, SearchOption options) => Directory.GetFileSystemEntries(path, searchPattern, options);
        
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public string CompressFile(string path) {
            string compressedFilePath = Path.GetTempFileName();
            using (FileStream sourceFileStream = File.OpenRead(path))
            using (FileStream compressedFileStream = File.Create(compressedFilePath))
            using (GZipStream stream = new GZipStream(compressedFileStream, CompressionLevel.Optimal)) {
                // 81920 is the default compression buffer size
                sourceFileStream.CopyTo(compressedFileStream, 81920);
            }

            return compressedFilePath;
        }

        public string CompressDirectory(string path) {
            Matcher matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
            matcher.AddInclude("*.*");
            return CompressDirectory(path, matcher, new Progress<string>((p) => { }), CancellationToken.None);
        }

        public string CompressDirectory(string path, Matcher matcher, IProgress<string> progress, CancellationToken ct) {
            string zipFilePath = Path.GetTempFileName();
            using (FileStream zipStream = new FileStream(zipFilePath, FileMode.Create)) 
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create)) {
                Queue<string> dirs = new Queue<string>();
                dirs.Enqueue(path);
                while (dirs.Count > 0) {
                    var dir = dirs.Dequeue();
                    var subdirs = Directory.GetDirectories(dir);
                    foreach(var subdir in subdirs) {
                        dirs.Enqueue(subdir);
                    }

                    var files = matcher.GetResultsInFullPath(dir);
                    foreach (var file in files) {
                        if (ct.IsCancellationRequested) {
                            return string.Empty;
                        }
                        progress?.Report(file);
                        string entryName = file.MakeRelativePath(dir).Replace('\\', '/');
                        archive.CreateEntryFromFile(file, entryName);
                    }
                }
            }
            return zipFilePath;
        }

        public string GetDownloadsPath(string fileName) {
            if (string.IsNullOrWhiteSpace(fileName)) {
                return GetKnownFolderPath(KnownFolderGuids.Downloads);
            } else {
                return Path.Combine(GetKnownFolderPath(KnownFolderGuids.Downloads), fileName);
            }
        }

        private string GetKnownFolderPath(string knownFolder) {
            IntPtr knownFolderPath;
            uint flags = (uint)NativeMethods.KnownFolderflags.KF_FLAG_DEFAULT_PATH;
            int result = NativeMethods.SHGetKnownFolderPath(new Guid(knownFolder), flags, IntPtr.Zero, out knownFolderPath);
            if (result >= 0) {
                return Marshal.PtrToStringUni(knownFolderPath);
            } else {
                return string.Empty;
            }
        }

        private static class NativeMethods {
            public const int MAX_PATH = 260;

            [Flags]
            public enum KnownFolderflags : uint {
                KF_FLAG_DEFAULT = 0x00000000,
                KF_FLAG_SIMPLE_IDLIST = 0x00000100,
                KF_FLAG_NOT_PARENT_RELATIVE = 0x00000200,
                KF_FLAG_DEFAULT_PATH = 0x00000400,
                KF_FLAG_INIT = 0x00000800,
                KF_FLAG_NO_ALIAS = 0x00001000,
                KF_FLAG_DONT_UNEXPAND = 0x00002000,
                KF_FLAG_DONT_VERIFY = 0x00004000,
                KF_FLAG_CREATE = 0x00008000,
                KF_FLAG_NO_APPCONTAINER_REDIRECTION = 0x00010000,
                KF_FLAG_ALIAS_ONLY = 0x80000000,
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern uint GetLongPathName([MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                                                      [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer,
                                                      int nBufferLength);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern uint GetShortPathName([MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                                                       [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer,
                                                       int nBufferLength);

            [DllImport("Shell32.dll")]
            public static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, 
                                                          uint dwFlags, 
                                                          IntPtr hToken,
                                                          out IntPtr ppszPath);
        }
    }
}