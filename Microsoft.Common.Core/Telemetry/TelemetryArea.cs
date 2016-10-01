// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace Microsoft.Common.Core.Telemetry {
    /// <summary>
    /// Area names show up as part of telemetry event names like:
    ///   VS/RTools/[area]/[event]
    public enum TelemetryArea {
        // Keep these sorted
        Build,
        Configuration,
        DataGrid,
        Debugger,
        Editor,
        History,
        Options,
        Packages,
        Plotting,
        Project,
        Repl,
        SQL,
        VariableExplorer,
    }
}
