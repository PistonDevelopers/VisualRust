// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.FormattableString;

namespace Microsoft.Common.Core {
    public static class ClipboardExtensions {
        public static void CopyToClipboard(this string data) {
            try {
                Clipboard.SetData(DataFormats.UnicodeText, Invariant($"\"{data}\""));
            } catch (ExternalException) { }
        }
    }
}
