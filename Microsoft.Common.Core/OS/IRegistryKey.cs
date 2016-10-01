// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.OS {
    public interface IRegistryKey : IDisposable {
        object GetValue(string name);
        void SetValue(string name, object value);
        string[] GetSubKeyNames();
        IRegistryKey OpenSubKey(string name, bool writable = false);
    }
}
