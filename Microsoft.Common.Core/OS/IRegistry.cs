// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.OS {
    public interface IRegistry {
        IRegistryKey OpenBaseKey(Win32.RegistryHive hive, Win32.RegistryView view);
    }
}
