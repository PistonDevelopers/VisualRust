// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.Common.Core.OS {
    public static class ErrorCodeConverter {
        public static string MessageFromErrorCode(int errorCode) {
            // Twy Win32 first
            uint flags = NativeMethods.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeMethods.FORMAT_MESSAGE_FROM_SYSTEM | NativeMethods.FORMAT_MESSAGE_IGNORE_INSERTS;
            IntPtr lpMsgBuf = IntPtr.Zero;

            var length = NativeMethods.FormatMessage(flags, IntPtr.Zero, errorCode, 0, ref lpMsgBuf, 0, IntPtr.Zero);
            if (length == 0) {
                // If failed, try to convert NTSTATUS to Win32 error
                int code = NativeMethods.RtlNtStatusToDosError(errorCode);
                return new Win32Exception(code).Message;
            }

            string message = null;
            if (lpMsgBuf != IntPtr.Zero) {
                message = Marshal.PtrToStringUni(lpMsgBuf);
                Marshal.FreeHGlobal(lpMsgBuf);
            }
            return message ?? string.Empty;
        }
    }
}
