// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Common.Core {
    public static class DictionaryExtension {
        public static IDictionary<string, object> FromAnonymousObject(object data) {
            IDictionary<string, object> dict;
            if (data != null) {
                dict = data as IDictionary<string, object>;
                if (dict == null) {
                    var attr = BindingFlags.Public | BindingFlags.Instance;
                    dict = new Dictionary<string, object>();
                    foreach (var property in data.GetType().GetProperties(attr)) {
                        if (property.CanRead) {
                            dict.Add(property.Name, property.GetValue(data, null));
                        }
                    }
                }
            }
            else {
                dict = new Dictionary<string, object>();
            }
            return dict;
        }

        public static void RemoveWhere<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, bool> predicate) {
            var toRemove = dictionary.Where(predicate).ToList();
            foreach (var item in toRemove) {
                dictionary.Remove(item.Key);
            }
        }
    }
}
