// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.Logging {
    public interface ILoggingPermissions {
        /// <summary>
        /// Defines maximum allowable logging level.
        /// </summary>
        LogVerbosity MaxVerbosity { get; }

        /// <summary>
        /// Is user permitted to send feedback
        /// </summary>
        bool IsFeedbackPermitted { get; }

        /// <summary>
        /// Currently set logging level (usually via Tools | Options). 
        /// Cannot exceeed maximum level.
        /// </summary>
        LogVerbosity CurrentVerbosity { get; set; }
    }
}
