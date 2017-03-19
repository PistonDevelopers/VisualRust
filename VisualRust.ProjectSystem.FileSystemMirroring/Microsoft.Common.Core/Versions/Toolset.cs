// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core {
    public static class Toolset {
#if VS14
        public const string Version = "14.0";
#endif
#if VS15
        public const string Version = "15.0";
#endif
    }
}
