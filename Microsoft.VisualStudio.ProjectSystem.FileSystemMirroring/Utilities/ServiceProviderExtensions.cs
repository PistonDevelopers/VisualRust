// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities {
    public static class ServiceProviderExtensions {
        public static Lazy<T> GetServiceLazy<T>(this IServiceProvider serviceProvider, Type serviceType = null, LazyThreadSafetyMode mode = LazyThreadSafetyMode.ExecutionAndPublication)
            where T : class {
            serviceType = serviceType ?? typeof(T);
            return new Lazy<T>(() => serviceProvider.GetService(serviceType) as T, mode);
        }
    }
}
