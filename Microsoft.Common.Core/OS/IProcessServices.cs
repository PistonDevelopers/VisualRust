// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Microsoft.Common.Core.OS {
    public interface IProcessServices {
        void Start(ProcessStartInfo psi);
        void Start(string path);
    }
}
