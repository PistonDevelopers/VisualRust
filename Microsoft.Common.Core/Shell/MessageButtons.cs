// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Shell {

    /// <summary>
    /// Specifies which buttons to show in a message box.
    /// Also used as a return value to tell which button 
    /// was pressed.
    /// </summary>
    [Flags]
    public enum MessageButtons {
        OK = 0,
        Cancel = 0x01,
        Yes = 0x02,
        No = 0x04,
        OKCancel = OK | Cancel,
        YesNo = Yes | No,
        YesNoCancel = YesNo | Cancel,
    }
}