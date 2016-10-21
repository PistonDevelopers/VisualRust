// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Extensions {
    public static class MathExtensions {
        public static T Min<T>(T a, T b) where T : IComparable {
            return a.CompareTo(b) <= 0 ? a : b;
        }
        public static T Max<T>(T a, T b) where T : IComparable {
            return a.CompareTo(b) > 0 ? a : b;
        }
    }
}
