// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Threading {
    public interface IMainThread {
        int ThreadId { get; }
        void Post(Action action);
    }
}