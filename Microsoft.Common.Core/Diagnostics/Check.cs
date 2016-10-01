// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Diagnostics {
    public static class Check {
        public static void ArgumentNull(string argumentName, object argument) {
            if (argument == null) {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void ArgumentStringNullOrEmpty(string argumentName, string argument) {
            Check.ArgumentNull(argumentName, argument);

            if (string.IsNullOrEmpty(argument)) {
                throw new ArgumentException(argumentName);
            }
        }

        public static void ArgumentOutOfRange(string argumentName, Func<bool> predicate) {
            if (predicate()) {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void InvalidOperation(Func<bool> predicate) {
            if (predicate()) {
                throw new InvalidOperationException();
            }
        }
    }
}