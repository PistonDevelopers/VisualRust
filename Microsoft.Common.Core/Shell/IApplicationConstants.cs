// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Shell {
    /// <summary>
    /// Defines application constants such as locale, registry key names, etc.
    /// Implemented by the host application. Imported via MEF.
    /// </summary>
    public interface IApplicationConstants {
        /// <summary>
        /// Application name to use in log, system events, etc.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Application locale ID (LCID)
        /// </summary>
        uint LocaleId { get; }

        /// <summary>
        /// Root of HLKM application hive for admin-level settings.
        /// </summary>
        string LocalMachineHive { get; }

        /// <summary>
        /// Application top level window handle. Typically used as a parent for native dialogs.
        /// </summary>
        IntPtr ApplicationWindowHandle { get; }

        UIColorTheme UIColorTheme { get; }
    }
}
