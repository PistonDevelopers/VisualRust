// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Telemetry {
    /// <summary>
    /// Represents object that records telemetry events and is called by
    /// the telemetry service. In Visual Studio environment maps to IVsTelemetryService
    /// whereas in tests can be replaced by an object that writes events to a string.
    /// </summary>
    public interface ITelemetryRecorder : IDisposable {
        /// <summary>
        /// True if telemetry is actually being recorded
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Indicates if telemetry can collect private information
        /// </summary>
        bool CanCollectPrivateInformation { get; }

        /// <summary>
        /// Records event with parameters. Perameters are
        /// a collection of string/object pairs.
        /// </summary>
        void RecordEvent(string eventName, object parameters = null);
    }
}
