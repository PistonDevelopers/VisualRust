// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Microsoft.Common.Core.OS {
    public sealed class ProcessServices : IProcessServices {
        public Process Start(ProcessStartInfo psi) {
            return Process.Start(psi);
        }

        public Process Start(string path) {
            return Process.Start(path);
        }
    }
}
