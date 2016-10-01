// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.Threading {
    public struct BackgroundThreadAwaitable {
        public BackgroundThreadAwaiter GetAwaiter() {
            return new BackgroundThreadAwaiter();
        }
    }
}