// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Microsoft.Common.Core.OS {
    public sealed class ProcessServices : IProcessServices {
        private static IProcessServices _instance;

        public static IProcessServices Current {
            get {
                if (_instance == null) {
                    _instance = new ProcessServices();
                }
                return _instance;
            }
            internal set { _instance = value; }
        }

        public void Start(ProcessStartInfo psi) {
            Process.Start(psi);
        }

        public void Start(string path) {
            Process.Start(path);
        }
    }
}
