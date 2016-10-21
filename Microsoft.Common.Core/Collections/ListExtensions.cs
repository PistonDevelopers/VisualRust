// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Common.Core.Collections {
    public static class ListExtensions {
        public static IList<T> AddIf<T>(this IList<T> list, bool condition, T value) {
            if (condition) {
                list.Add(value);
            }

            return list;
        }

        public static void RemoveWhere<T>(this IList<T> list, Func<T, bool> predicate) {
            for (var i = list.Count - 1; i >= 0; i--) {
                if (predicate(list[i])) {
                    list.RemoveAt(i);
                }
            }
        }

        public static bool AddSorted<T>(this IList<T> list, T value, IComparer<T> comparer = null) {
            var index = list.BinarySearch(value, comparer);
            if (index >= 0) {
                return false;
            }

            list.Insert(~index, value);
            return true;
        }

        public static bool RemoveSorted<T>(this IList<T> list, T value, IComparer<T> comparer = null) {
            var index = list.BinarySearch(value, comparer);
            if (index < 0) {
                return false;
            }

            list.RemoveAt(index);
            return true;
        }

        public static int BinarySearch<T>(this IList<T> list, T value, IComparer<T> comparer = null) {
            if (list == null) {
                throw new ArgumentNullException(nameof(list));
            }

            comparer = comparer ?? Comparer<T>.Default;

            int low = 0;
            int high = list.Count - 1;

            while (low <= high) {
                int mid = low + (high - low) / 2;
                int comparisonResult = comparer.Compare(list[mid], value);

                if (comparisonResult < 0) {
                    low = mid + 1;
                } else if (comparisonResult > 0) {
                    high = mid - 1;
                } else {
                    return mid;
                }
            }

            return ~low;
        }

        public static bool Equals<T, TOther>(this IList<T> source, IList<TOther> other, Func<T, TOther, bool> predicate) {
            return source.Count == other.Count && !source.Where((t, i) => !predicate(t, other[i])).Any();
        }
    }
}