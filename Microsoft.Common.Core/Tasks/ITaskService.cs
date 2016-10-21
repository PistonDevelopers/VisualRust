// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Tasks {
    public interface ITaskService {
        bool Wait(Task task, CancellationToken cancellationToken = default(CancellationToken), int ms = Timeout.Infinite);
    }
}
