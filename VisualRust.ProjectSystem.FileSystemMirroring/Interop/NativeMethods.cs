// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using System.Text;

namespace VisualRust.ProjectSystem.FileSystemMirroring.Interop {
    internal static class NativeMethods {
        public const int MAX_PATH = 260;
 
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetLongPathName([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, 
                                                  [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer, 
                                                  int nBufferLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetShortPathName([MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                                                   [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer,
                                                   int nBufferLength);
    }
}
