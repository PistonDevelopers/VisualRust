// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualRust.ProjectSystem.FileSystemMirroring.Utilities {
    public static class DictionaryExtensions {
        public static TKey GetFirstKeyByValueIgnoreCase<TKey>(this IDictionary<TKey, string> dictionary, string value) {
            return dictionary.GetFirstKeyByValue(value, StringComparer.OrdinalIgnoreCase);
        }

        public static TKey GetFirstKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value, IEqualityComparer<TValue> comparer) {
            comparer = comparer ?? EqualityComparer<TValue>.Default;
            return dictionary.FirstOrDefault(kvp => comparer.Equals(kvp.Value, value)).Key;
        }
    }
}
