using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ManuPath.Extensions
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> Each<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
            return collection;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection?.Any() != true;
        }

        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection?.Any() == true;
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> collection)
            => collection ?? Enumerable.Empty<T>();

        public static T[] EmptyIfNull<T>(this T[] arr)
            => arr ?? Array.Empty<T>();

    }
}
