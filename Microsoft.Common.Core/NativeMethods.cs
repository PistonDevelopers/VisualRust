// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Common.Core {
    internal static unsafe class NativeMethods {
        public const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        public const uint FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        public const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        public const uint FORMAT_MESSAGE_FROM_STRING = 0x00000400;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, int dwMessageId, 
             uint dwLanguageId, ref IntPtr lpBuffer, uint nSize, IntPtr pArguments);

        [DllImport("ntdll.dll")]
        public static extern int RtlNtStatusToDosError(int Status);


        public const int CRED_MAX_USERNAME_LENGTH = 513;
        public const int CRED_MAX_CREDENTIAL_BLOB_SIZE = 512;
        public const int CREDUI_MAX_USERNAME_LENGTH = CRED_MAX_USERNAME_LENGTH;
        public const int CREDUI_MAX_PASSWORD_LENGTH = (CRED_MAX_CREDENTIAL_BLOB_SIZE / 2);

        public const int CREDUI_FLAGS_INCORRECT_PASSWORD = 0x1;
        public const int CREDUI_FLAGS_DO_NOT_PERSIST = 0x2;
        public const int CREDUI_FLAGS_REQUEST_ADMINISTRATOR = 0x4;
        public const int CREDUI_FLAGS_EXCLUDE_CERTIFICATES = 0x8;
        public const int CREDUI_FLAGS_REQUIRE_CERTIFICATE = 0x10;
        public const int CREDUI_FLAGS_SHOW_SAVE_CHECK_BOX = 0x40;
        public const int CREDUI_FLAGS_ALWAYS_SHOW_UI = 0x80;
        public const int CREDUI_FLAGS_REQUIRE_SMARTCARD = 0x100;
        public const int CREDUI_FLAGS_PASSWORD_ONLY_OK = 0x200;
        public const int CREDUI_FLAGS_VALIDATE_USERNAME = 0x400;
        public const int CREDUI_FLAGS_COMPLETE_USERNAME = 0x800;
        public const int CREDUI_FLAGS_PERSIST = 0x1000;
        public const int CREDUI_FLAGS_SERVER_CREDENTIAL = 0x4000;
        public const int CREDUI_FLAGS_EXPECT_CONFIRMATION = 0x20000;
        public const int CREDUI_FLAGS_GENERIC_CREDENTIALS = 0x40000;
        public const int CREDUI_FLAGS_USERNAME_TARGET_CREDENTIALS = 0x80000;
        public const int CREDUI_FLAGS_KEEP_USERNAME = 0x100000;

        [DllImport("credui", CharSet = CharSet.Auto)]
        public static extern int CredUIPromptForCredentials(
            ref CREDUI_INFO pUiInfo,
            string pszTargetName,
            IntPtr Reserved,
            int dwAuthError,
            StringBuilder pszUserName,
            int ulUserNameMaxChars,
            IntPtr pszPassword,
            int ulPasswordMaxChars,
            ref bool pfSave,
            int dwFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct CREDUI_INFO {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }
    }
}
