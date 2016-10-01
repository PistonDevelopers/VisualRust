// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Common.Core {
    public static class EnumerableExtensions {
        public static List<T> AsList<T>(this IEnumerable<T> source) {
            return source as List<T> ?? source.ToList();
        }

        public static T[] AsArray<T>(this IEnumerable<T> source) {
            return source as T[] ?? source.ToArray();
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item) {
            foreach (T sourceItem in source) {
                yield return sourceItem;
            }

            yield return item;
        }

        public static void Split<T>(this IEnumerable<T> source, Func<T, bool> predicate, out IList<T> first, out IList<T> second) {
            first = new List<T>();
            second = new List<T>();
            foreach (var item in source) {
                if (predicate(item)) {
                    first.Add(item);
                } else {
                    second.Add(item);
                }
            }
        }

        public static void Split<TIn, TOut>(this IEnumerable<TIn> source, Func<TIn, bool> predicate, Func<TIn, TOut> converter, out IList<TOut> first, out IList<TOut> second) {
            first = new List<TOut>();
            second = new List<TOut>();
            foreach (var item in source) {
                if (predicate(item)) {
                    first.Add(converter(item));
                } else {
                    second.Add(converter(item));
                }
            }
        }

        public static IEnumerable<IReadOnlyCollection<T>> Split<T>(this IEnumerable<T> source, int chunkSize) {
            var index = 0;
            var items = new T[chunkSize];
            foreach (var item in source) {
                items[index] = item;
                index++;

                if (index == chunkSize) {
                    index = 0;
                    yield return items;
                    items = new T[chunkSize];
                }
            }

            if (index > 0) {
                T[] lastItems = new T[index];
                Array.Copy(items, 0, lastItems, 0, lastItems.Length);
                yield return lastItems;
            }
        }

        public static IEnumerable<int> IndexWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
            var i = 0;
            foreach (var item in source) {
                if (predicate(item)) {
                    yield return i;
                }

                i++;
            }
        }

        public static IEnumerable<T> TraverseBreadthFirst<T>(this T root, Func<T, IEnumerable<T>> selectChildren) {
            Queue<T> items = new Queue<T>();
            items.Enqueue(root);
            while (items.Count > 0) {
                var item = items.Dequeue();
                yield return item;

                IEnumerable<T> childen = selectChildren(item);
                if (childen == null) {
                    continue;
                }

                foreach (var child in childen) {
                    items.Enqueue(child);
                }
            }
        }

        public static IEnumerable<T> TraverseDepthFirst<T>(this T root, Func<T, IEnumerable<T>> selectChildren) {
            yield return root;

            var children = selectChildren(root);
            if (children != null) {
                foreach (T child in children) {
                    foreach (T t in TraverseDepthFirst(child, selectChildren)) {
                        yield return t;
                    }
                }
            }
        }
    }
}
